using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;
using System.Threading;
using System.IO;

//Standard Exception Types: https://msdn.microsoft.com/library/ms229007(v=vs.100).aspx
//See Also
//https://github.com/michael-wolfenden/Polly
//http://www.hanselman.com/blog/NuGetPackageOfTheWeekPollyWannaFluentlyExpressTransientExceptionHandlingPoliciesInNET.aspx
//http://putridparrot.com/blog/exception-handling-policies-with-polly/


namespace ScottsExceptionHarness
{
	class Program
	{
		static void Main(string[] args)
		{


			// Retry, waiting a specified duration between each retry
			var policy = Policy
			  .Handle<DivideByZeroException>()
			  .WaitAndRetry(new[]
					{
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(3),
					TimeSpan.FromSeconds(5),
					TimeSpan.FromSeconds(10)
				  }, (ex, ts, context) =>
				  {
					  Console.WriteLine("ts : " + ts + "; ex: " + ex.Message);
				  });

			var policy2 = Policy.Handle<DivideByZeroException>().Retry(3, (exception, retryCount) =>
			{
				Console.WriteLine("Retry Count : " + retryCount);
			});

			var policy2b = Policy.Handle<DivideByZeroException>().Or<FileNotFoundException>().Retry(3, (exception, retryCount) =>
			{
				Console.WriteLine("Retry Count : " + retryCount);
			});

			var policy2c = Policy.Handle<ArgumentException>(ex => ex.ParamName == "example").Retry(3, (exception, retryCount) =>
			{
				Console.WriteLine("Named param Argument Exception - Retry Count : " + retryCount);
			});

			//the action (timespan, retryattempt) only takes 3 params...this was originally using "context" as the last param
			var policy2d = Policy.Handle<DivideByZeroException>().WaitAndRetry(5,retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),(exception, timeSpan, retryAttempt) =>
			{
				Console.WriteLine("Divide by Zero Exception - Calculated Retry Count: " + retryAttempt + "; Timespan: " + timeSpan);
			});



			//before it resets, can't execute again
			var policy3 = Policy.Handle<DivideByZeroException>()
				.CircuitBreaker(3, TimeSpan.FromSeconds(10));

			ScottService service = new ScottService();

			/////////////////////////////////////////////////////
			///Policy 1 - Retry w/slower intervals
			/////////////////////////////////////////////////////
			try
			{
				int x;
				int y = 6;
				int z = 0;
				//policy.Execute(() => x = y/z);
				policy.Execute(() => service.remotething(y, z));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine();
			}

			/////////////////////////////////////////////////////
			///Policy 2 - Retry 3 times
			/////////////////////////////////////////////////////
			try
			{
				int x;
				int y = 6;
				int z = 0;
				//policy.Execute(() => x = y/z);
				policy2.Execute(() => service.remotething(y, z));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine();
			}

			/////////////////////////////////////////////////////
			///Policy 2b - Retry 3 times w/chaining
			/////////////////////////////////////////////////////
			try
			{
				policy2b.Execute(() => service.fileNotFoundException());
			}
			catch (Exception e)
			{
				Console.WriteLine("Threw FileNotFoundException for " + e.Message);
				Console.WriteLine();
			}

			/////////////////////////////////////////////////////
			///Policy 2c - Parameterized Retry 3 times w/chaining
			/////////////////////////////////////////////////////
			try
			{
				policy2c.Execute(() => service.toThrowANamedArgumentException("",3,"green"));
			}
			catch (ArgumentException e)
			{
				//should retry, it's named
				Console.WriteLine("Threw ArgumentException for " + e.ParamName);
				Console.WriteLine();
			}
			try
			{
				//this shouldn't retry because it's not named.
				policy2c.Execute(() => service.toThrowANamedArgumentException("blue", 3, ""));
			}
			catch (ArgumentException e)
			{
				Console.WriteLine("Threw ArgumentException for " + e.ParamName);
				Console.WriteLine();
			}

			/////////////////////////////////////////////////////
			///Policy 2d - Calculated increase
			/////////////////////////////////////////////////////
			try
			{
				int x;
				int y = 6;
				int z = 0;
				//policy.Execute(() => x = y/z);
				policy2d.Execute(() => service.remotething(y, z));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine();
			}

			/////////////////////////////////////////////////////
			///Policy 3 - Circuit Breaker
			/////////////////////////////////////////////////////
			for (int i = 0; i < 50; i++)
			{
				//to show the Circuit Breaker (policy3) in action - remove this and the loop for policy1 and policy 2
				Thread.Sleep(1000);
				try
				{
					int x;
					int y = 6;
					int z = 0;
					//policy.Execute(() => x = y/z);
					policy3.Execute(() => service.remotething(y, z));
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					//Console.WriteLine();
				}

			}


		}
	}
	}

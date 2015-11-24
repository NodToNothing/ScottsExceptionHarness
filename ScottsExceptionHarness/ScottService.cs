using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;	

namespace ScottsExceptionHarness
{
	class ScottService
	{
		public int remotething(int a, int b)
		{
			return a / b;

		}

		public int fileNotFoundException()
		{
			throw new FileNotFoundException(@"[Schulke_so_embarrassing.jpg]");
		}

		public string toThrowANamedArgumentException(string example, int x, string other)
		{
			if (example == "")
			{
				throw new ArgumentException("force an example error", "example");
			}
			else if (other == "")
			{
				throw new ArgumentException("force an other error", "other");
			}
			
			return example + other + x.ToString();
		}
	}
}

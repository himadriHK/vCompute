using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeLoader;

namespace SimpleClass
{
    public class Simple:CodeInterface
    {
		public string doSomething(string param)
		{
			return param.PadLeft(20,'@');
		}

		public string DoWork(object param)
		{
			return ((string)param).PadLeft(20, '@');
		}
	}
}

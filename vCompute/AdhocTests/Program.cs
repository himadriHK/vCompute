﻿using CommAPI;
using System;


namespace AdhocTests
{
	class Program
	{
		static void Main(string[] args)
		{
			//Loader loader = new Loader(@"C:\Users\Himadri-HK\Documents\Visual Studio 2015\Projects\CodeLoader\code.bin");
			//CodeFileSystem cfs = loader.codeDictionary;
			//Console.WriteLine(cfs.ToString());
			//FileStream fs = new FileStream(@"C:\Users\Himadri-HK\Source\Repos\vCompute\vCompute\SimpleClass\bin\Debug\SimpleClass.dll",FileMode.Open);
			//byte[] buffer=new byte[fs.Length];
			//fs.Read(buffer, 0, (int)fs.Length);
			//cfs.writeAssembly("SimpleClass", buffer);
			//Assembly asm = Assembly.Load(cfs.readAssembly("SimpleClass"));
			//CodeInterface obj = null; ;
			//foreach (Type t in asm.GetTypes())
			//	if(t.GetInterface("CodeInterface")!=null)
			//	 obj= (CodeInterface)Activator.CreateInstance(t);
			//Console.WriteLine(obj.DoWork("Yes"));
			//loader.saveCodeDictionary();
			//Console.Read();
			///Server server = new Server(8888);
			Client client = new Client("ec2-18-216-239-133.us-east-2.compute.amazonaws.com", 8888, "himadriHK",AppDomain.CurrentDomain.BaseDirectory+ @"client.bin");
			Console.ReadLine();
		}
	}
}

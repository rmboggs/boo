namespace Boo.Tests.Ast.Compiler
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using Boo.Lang.Ast;
	using Boo.Lang.Compiler;
	using Boo.Lang.Compiler.IO;
	using Boo.Lang.Compiler.Pipeline;
	using NUnit.Framework;
	
	public abstract class AbstractCompilerTestCase
	{
		protected BooCompiler _compiler;
		
		protected CompilerParameters _parameters;
		
		protected string _baseTestCasesPath;
		
		[TestFixtureSetUp]
		public virtual void SetUpFixture()
		{
			 Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Error));

			string booAssemblyPath = typeof(BooCompiler).Assembly.Location;
			string thisAssemblyPath = GetType().Assembly.Location;
			File.Copy(booAssemblyPath, Path.Combine(Path.GetTempPath(), Path.GetFileName(booAssemblyPath)), true);
			File.Copy(thisAssemblyPath, Path.Combine(Path.GetTempPath(), Path.GetFileName(thisAssemblyPath)), true);
			
			_baseTestCasesPath = Path.Combine(BooTestCaseUtil.TestCasesPath, "compilation");
			
			_compiler = new BooCompiler();
			_parameters = _compiler.Parameters;
			//_parameters.TraceSwitch.Level = TraceLevel.Verbose;
			_parameters.OutputAssembly = Path.Combine(Path.GetTempPath(), "testcase.exe");
			SetUpCompilerPipeline(_parameters.Pipeline);
			
		}
		
		[TestFixtureTearDown]
		public virtual void TearDownFixture()
		{
			Trace.Listeners.Clear();
		}
		
		[SetUp]
		public virtual void SetUpTest()
		{
			_parameters.Input.Clear();
		}		
		
		protected abstract void SetUpCompilerPipeline(CompilerPipeline pipeline);
		
		protected void RunCompilerTestCase(string name)
		{
			RunCompilerTestCase(name, string.Empty);
		}
		
		protected void RunCompilerTestCase(string name, string description)
		{			
			_parameters.Input.Add(new FileInput(GetTestCasePath(name)));
			
			CompilerContext context;
			string output = Run(null, out context);
			Assert.AreEqual(_parameters.Input.Count, context.CompileUnit.Modules.Count, "compilation must generate as many modules as were compiler inputs");
			string expected = context.CompileUnit.Modules[0].Documentation;
			Assert.AreEqual(expected.Trim(), output.Trim(), description);
		}
		
		protected string RunString(string code)
		{	
			return RunString(code, null);
		}
		
		protected string RunString(string code, string stdin)
		{
			_parameters.Input.Add(new StringInput("<teststring>", code));
			
			CompilerContext context;
			return Run(stdin, out context);
		}
		
		protected string Run(string stdin, out CompilerContext context)
		{
			TextWriter oldStdOut = Console.Out;
			TextReader oldStdIn = Console.In;
			
			try
			{
				StringWriter console = new StringWriter();
				Console.SetOut(console);
				if (null != stdin)
				{
					Console.SetIn(new StringReader(stdin));
				}
				
				context = _compiler.Run();
				
				if (context.Errors.Count > 0)
				{
					Assert.Fail(context.Errors.ToString(false));
				}
				return console.ToString().Replace("\r\n", "\n");
			}
			finally
			{				
				Console.SetOut(oldStdOut);
				if (null != stdin)
				{
					Console.SetIn(oldStdIn);
				}
			}
		}
		
		protected virtual string GetTestCasePath(string fname)
		{
			return Path.Combine(_baseTestCasesPath, fname);
		}
	}
}

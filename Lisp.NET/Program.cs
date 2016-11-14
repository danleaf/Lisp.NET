using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Microsoft.FSharp.Collections;
using System.Xml;
using System.IO;
using Lexer;

using intlist = Microsoft.FSharp.Collections.FSharpList<int>;

namespace mylisp
{
    class Program
    {
        public static void CreateDynamicType()
        {
            Type[] ctorParams = new Type[] {typeof(int),
				   typeof(int)};

            AppDomain myDomain = Thread.GetDomain();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "MyDynamicAssembly";

            AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(
                           myAsmName,
                           AssemblyBuilderAccess.RunAndSave);

            ModuleBuilder pointModule = myAsmBuilder.DefineDynamicModule("PointModule",
                                         "Point.dll");
           
            TypeBuilder pointTypeBld = pointModule.DefineType("Lisp.Point",
                                       TypeAttributes.Public);

            FieldBuilder xField = pointTypeBld.DefineField("x", typeof(int),
                                                           FieldAttributes.Public);
            FieldBuilder yField = pointTypeBld.DefineField("y", typeof(int),
                                                           FieldAttributes.Public);


            Type objType = Type.GetType("System.Object");
            ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);

            ConstructorBuilder pointCtor = pointTypeBld.DefineConstructor(
                                        MethodAttributes.Public,
                                        CallingConventions.Standard,
                                        ctorParams);
            ILGenerator ctorIL = pointCtor.GetILGenerator();


            // First, you build the constructor.
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, objCtor);
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_1);
            ctorIL.Emit(OpCodes.Stfld, xField);
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_2);
            ctorIL.Emit(OpCodes.Stfld, yField);
            ctorIL.Emit(OpCodes.Ret);

            //  Now, you'll build a method to output some information on the
            // inside your dynamic class. This method will have the following
            // definition in C#:
            //  public void WritePoint()

            MethodBuilder writeStrMthd = pointTypeBld.DefineMethod(
                                          "WritePoint",
                                  MethodAttributes.Public,
                                                  typeof(void),
                                                  null);


            ILGenerator writeStrIL = writeStrMthd.GetILGenerator();

            // The below ILGenerator created demonstrates a few ways to create
            // string output through STDIN. 

            // ILGenerator.EmitWriteLine(string) will generate a ldstr and a 
            // call to WriteLine for you.

            writeStrIL.EmitWriteLine("The value of this current instance is:");

            // Here, you will do the hard work yourself. First, you need to create
            // the string we will be passing and obtain the correct WriteLine overload
            // for said string. In the below case, you are substituting in two values,
            // so the chosen overload is Console.WriteLine(string, object, object).

            String inStr = "({0}, {1})";
            Type[] wlParams = new Type[] {typeof(string),
				     typeof(object),
				     typeof(object)};

            // We need the MethodInfo to pass into EmitCall later.

            MethodInfo writeLineMI = typeof(Console).GetMethod(
                                 "WriteLine",
                             wlParams);

            // Push the string with the substitutions onto the stack.
            // This is the first argument for WriteLine - the string one. 

            writeStrIL.Emit(OpCodes.Ldstr, inStr);

            // Since the second argument is an object, and it corresponds to
            // to the substitution for the value of our integer field, you 
            // need to box that field to an object. First, push a reference
            // to the current instance, and then push the value stored in
            // field 'x'. We need the reference to the current instance (stored
            // in local argument index 0) so Ldfld can load from the correct
            // instance (this one).

            writeStrIL.Emit(OpCodes.Ldarg_0);
            writeStrIL.Emit(OpCodes.Ldfld, xField);

            // Now, we execute the box opcode, which pops the value of field 'x',
            // returning a reference to the integer value boxed as an object.

            writeStrIL.Emit(OpCodes.Box, typeof(int));

            // Atop the stack, you'll find our string inStr, followed by a reference
            // to the boxed value of 'x'. Now, you need to likewise box field 'y'.

            writeStrIL.Emit(OpCodes.Ldarg_0);
            writeStrIL.Emit(OpCodes.Ldfld, yField);
            writeStrIL.Emit(OpCodes.Box, typeof(int));

            // Now, you have all of the arguments for your call to
            // Console.WriteLine(string, object, object) atop the stack:
            // the string InStr, a reference to the boxed value of 'x', and
            // a reference to the boxed value of 'y'.

            // Call Console.WriteLine(string, object, object) with EmitCall.

            writeStrIL.EmitCall(OpCodes.Call, writeLineMI, null);

            // Lastly, EmitWriteLine can also output the value of a field
            // using the overload EmitWriteLine(FieldInfo).

            writeStrIL.EmitWriteLine("The value of 'x' is:");
            writeStrIL.EmitWriteLine(xField);
            writeStrIL.EmitWriteLine("The value of 'y' is:");
            writeStrIL.EmitWriteLine(yField);

            // Since we return no value (void), the the ret opcode will not
            // return the top stack value.

            writeStrIL.Emit(OpCodes.Ret);


            Type type = pointTypeBld.CreateType();

            myAsmBuilder.Save("Point.dll");


            object[] _params = new object[2];

            string myX = "1";
            string myY = "2";

            Console.WriteLine("---");

            _params[0] = Convert.ToInt32(myX);
            _params[1] = Convert.ToInt32(myY);

            Type ptType = type;

            object ptInstance = Activator.CreateInstance(ptType, _params);
            ptType.InvokeMember("WritePoint",
                    BindingFlags.InvokeMethod,
                    null,
                    ptInstance,
                    new object[0]);

        }

        public static void Main()
        {
            Lexer.Lexer lexer = null;
            if (new FileInfo("d:/lexer.json").Exists)
            {
                lexer = Lexer.Lexer.LoadFromFile("d:/lexer.json");
                Console.WriteLine("read old");
            }
            else
            {
                lexer = new Lexer.Lexer(
                            new List<Regex>(){
                                new Regex("string", @"""([^""\r\n\\]*|\\.)*"""),
                                new Regex ("sepor",@";"),
                                new Regex("identifier",@"[a-zA-Z_][\w]*"),
                                new Regex("number",@"[0-9]+(\.[0-9]+)?"),
                                new Regex("point",@"\."),
                                new Regex("Error",@".")});
                Console.WriteLine("create new");
                lexer.SaveToFile("d:/lexer.json");
            }

            var result = lexer.GetTokenList(@"""qeqw\""qweqwe""sfsdf"";wewr;wer;""ew;r;3;4;234,4,to40430tf.sdfk48dldsf.d,f/sdf");
            foreach (var r in result)
                Console.WriteLine("{0}: {1}", r.Item1, r.Item2);
            Console.ReadKey();
        }

        public static string DafToXml(Dfa dfa)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("DFA");
            root.SetAttribute("StartID", dfa.StartID.ToString());
            doc.AppendChild(root);

            XmlElement nodes = doc.CreateElement("DfaNodes");
            root.AppendChild(nodes);

            foreach(var node in dfa.NodeList)
            {
                XmlElement xnode = doc.CreateElement("DfaNode");
                nodes.AppendChild(xnode);

                xnode.SetAttribute("ID", node.ID.ToString());
                xnode.SetAttribute("IsEnd", node.IsEnd.ToString());
                XmlElement xtranses = doc.CreateElement("Transitors");
                xnode.AppendChild(xtranses);

                foreach(var trans in node.Transitors)
                {
                    XmlElement xtrans = doc.CreateElement("Transitor");
                    xtranses.AppendChild(xtrans);

                    xtrans.SetAttribute("Dest", trans.Dest.ToString());
                    XmlElement input = doc.CreateElement("Inputs");
                    xtrans.AppendChild(input);

                    input.SetAttribute("Collection", "Opset");
                    input.SetAttribute("Type", "Char");

                    foreach (var v in trans.Input.ToList())
                    {
                        XmlElement xv = doc.CreateElement("Input");
                        input.AppendChild(xv);
                        var vs = v.ToString();
                        //switch (v)
                        //{
                        //    case '\t': vs = @"\t"; break;
                        //    case '\r': vs = @"\r"; break;
                        //    case '\n': vs = @"\n"; break;
                        //    case ' ': vs = @"\B"; break;
                        //}
                        xv.SetAttribute("Value", vs);
                    }
                }
            }
            doc.Save("D:\\abc.xml");
            return doc.InnerXml;  
        }
    }
}

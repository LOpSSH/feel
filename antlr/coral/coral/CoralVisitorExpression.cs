﻿using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coral
{
    partial class CoralVisitorBase
    {
        public override object VisitVariableStatement([NotNull] CoralParser.VariableStatementContext context)
        {
            var r1 = (Result)Visit(context.expression(0));
            var r2 = (Result)Visit(context.expression(1));
            var obj = "var " + r1.text + " = " + r2.text + context.Terminate().GetText() + Wrap;
            return obj;
        }

        public override object VisitAssignStatement([NotNull] CoralParser.AssignStatementContext context)
        {
            var r1 = (Result)Visit(context.expression(0));
            var r2 = (Result)Visit(context.expression(1));
            var obj = r1.text + " = " + r2.text + context.Terminate().GetText() + Wrap;
            return obj;
        }

        public override object VisitExpressionStatement([NotNull] CoralParser.ExpressionStatementContext context)
        {
            var r = (Result)Visit(context.expression());
            return r.text + context.Terminate().GetText() + Wrap;
        }

        public override object VisitExpression([NotNull] CoralParser.ExpressionContext context)
        {
            var count = context.ChildCount;
            var r = new Result();
            if(count == 2)
            {
                if(context.GetChild(1) is CoralParser.ReadElementContext)
                {
                    var ex = (Result)Visit(context.GetChild(0));
                    var read = (string)Visit(context.GetChild(1));
                    r.data = ex.data;
                    r.text = ex.text + read;
                }
            }
            else if(count == 3)
            {
                if(context.GetChild(1).GetType() == typeof(CoralParser.CallContext))
                {
                    r.data = "var";
                }
                else if(context.GetChild(1).GetType() == typeof(CoralParser.JudgeContext))
                {
                    // todo 如果左右不是bool类型值，报错
                    r.data = "bool";
                }
                else if(context.GetChild(1).GetType() == typeof(CoralParser.AddContext))
                {
                    // todo 如果左右不是number或text类型值，报错
                    r.data = "double";
                }
                else if(context.GetChild(1).GetType() == typeof(CoralParser.MulContext))
                {
                    // todo 如果左右不是number类型值，报错
                    r.data = "double";
                }
                var e1 = (Result)Visit(context.GetChild(0));
                var op = Visit(context.GetChild(1));
                var e2 = (Result)Visit(context.GetChild(2));
                r.text = e1.text + op + e2.text;
            }
            else if(count == 1)
            {
                r = (Result)Visit(context.GetChild(0));
            }
            return r;
        }

        public override object VisitCall([NotNull] CoralParser.CallContext context)
        {
            return context.op.Text;
        }

        public override object VisitWave([NotNull] CoralParser.WaveContext context)
        {
            return context.op.Text;
        }

        public override object VisitJudge([NotNull] CoralParser.JudgeContext context)
        {
            return context.op.Text;
        }

        public override object VisitAdd([NotNull] CoralParser.AddContext context)
        {
            return context.op.Text;
        }

        public override object VisitMul([NotNull] CoralParser.MulContext context)
        {
            return context.op.Text;
        }

        public override object VisitPrimaryExpression([NotNull] CoralParser.PrimaryExpressionContext context)
        {
            if(context.ChildCount == 1)
            {
                var c = context.GetChild(0);
                if(c is CoralParser.DataStatementContext)
                {
                    return Visit(context.dataStatement());
                }
                else if(c is CoralParser.IdContext)
                {
                    return Visit(context.id());
                }
                else if(context.t.Type == CoralParser.Self)
                {
                    return new Result { text = "this", data = "var" };
                }
                else if(context.t.Type == CoralParser.Discard)
                {
                    return new Result { text = "_", data = "var" };
                }
            }
            var r = (Result)Visit(context.expression());
            return new Result { text = "(" + r.text + ")", data = r.data };
        }

        public override object VisitExpressionList([NotNull] CoralParser.ExpressionListContext context)
        {
            var r = new Result();
            var obj = "";
            for(int i = 0; i < context.expression().Length; i++)
            {
                var temp = (Result)Visit(context.expression(i));
                if(i == 0)
                {
                    obj += temp.text;
                }
                else
                {
                    obj += ", " + temp.text;
                }
            }
            r.text = obj;
            r.data = "var";
            return r;
        }

        public override object VisitId([NotNull] CoralParser.IdContext context)
        {
            var r = new Result();
            r.data = "var";
            if(context.op.Type == CoralParser.IDPublic)
            {
                r.permission = "public";
            }
            else
            {
                r.permission = "private";
            }
            if(keywords.IndexOf(context.op.Text) >= 0)
            {
                r.text += "@";
            }
            r.text += context.op.Text;
            return r;
        }

        public override object VisitTemplateDefine([NotNull] CoralParser.TemplateDefineContext context)
        {
            var obj = "";
            obj += "<";
            for(int i = 0; i < context.id().Length; i++)
            {
                if(i > 0)
                {
                    obj += ",";
                }
                var r = (Result)Visit(context.id(i));
                obj += r.text;
            }
            obj += ">";
            return obj;
        }

        public override object VisitTemplateCall([NotNull] CoralParser.TemplateCallContext context)
        {
            var obj = "";
            obj += "<";
            for(int i = 0; i < context.type().Length; i++)
            {
                if(i > 0)
                {
                    obj += ",";
                }
                var r = Visit(context.type(i));
                obj += r;
            }
            obj += ">";
            return obj;
        }

        public override object VisitCallFunc([NotNull] CoralParser.CallFuncContext context)
        {
            var r = new Result();
            r.data = "var";
            var id = (Result)Visit(context.id());
            r.text += id.text;
            if(context.templateCall() != null)
            {
                r.text += Visit(context.templateCall());
            }
            r.text += Visit(context.tuple());
            return r;
        }

        public override object VisitCallPkg([NotNull] CoralParser.CallPkgContext context)
        {
            var r = new Result();
            r.data = Visit(context.type());
            r.text = "new " + Visit(context.type()) + Visit(context.tuple());
            return r;
        }

        public override object VisitArray([NotNull] CoralParser.ArrayContext context)
        {
            var type = "object";
            var result = new Result();
            for(int i = 0; i < context.expression().Length; i++)
            {
                var r = (Result)Visit(context.expression(i));
                if(i == 0)
                {
                    type = (string)r.data;
                    result.text += r.text;
                }
                else
                {
                    if(type != (string)r.data)
                    {
                        type = "object";
                    }
                    result.text += "," + r.text;
                }
            }
            result.data = "List<" + type + ">";
            result.text = "new List<" + type + ">(){" + result.text + "}";
            return result;
        }

        public override object VisitDictionary([NotNull] CoralParser.DictionaryContext context)
        {
            var key = "object";
            var value = "object";
            var result = new Result();
            for(int i = 0; i < context.dictionaryElement().Length; i++)
            {
                var r = (DicEle)Visit(context.dictionaryElement(i));
                if(i == 0)
                {
                    key = r.key;
                    value = r.value;
                    result.text += r.text;
                }
                else
                {
                    if(key != r.key)
                    {
                        key = "object";
                    }
                    if(value != r.value)
                    {
                        value = "object";
                    }
                    result.text += "," + r.text;
                }
            }
            var type = key + "," + value;
            result.data = "Dictionary<" + type + ">";
            result.text = "new Dictionary<" + type + ">(){" + result.text + "}";
            return result;
        }

        public override object VisitTypeConvert([NotNull] CoralParser.TypeConvertContext context)
        {
            var r = new Result();
            var data = Visit(context.type());
            var expr = (Result)Visit(context.expression());
            r.data = data;
            r.text = "(" + data + ")" + expr.text;
            return r;
        }

        public override object VisitVariableList([NotNull] CoralParser.VariableListContext context)
        {
            var newR = new Result();
            var r = (Result)Visit(context.expressionList());
            newR.text += "(" + r.text + ")";
            newR.data = "var";
            return newR;
        }

        class DicEle
        {
            public string key;
            public string value;
            public string text;
        }

        public override object VisitDictionaryElement([NotNull] CoralParser.DictionaryElementContext context)
        {
            var r1 = (Result)Visit(context.expression(0));
            var r2 = (Result)Visit(context.expression(1));
            var result = new DicEle();
            result.key = (string)r1.data;
            result.value = (string)r2.data;
            result.text = "{" + r1.text + "," + r2.text + "}";
            return result;
        }

        public override object VisitReadElement([NotNull] CoralParser.ReadElementContext context)
        {
            var obj = "";
            foreach(var item in context.expression())
            {
                var r = (Result)Visit(item);
                obj += "[" + r.text + "]";
            }
            return obj;
        }

        public override object VisitDataStatement([NotNull] CoralParser.DataStatementContext context)
        {
            var r = new Result();
            if(context.t.Type == CoralParser.Number)
            {
                r.data = "double";
                r.text = context.Number().GetText();
            }
            else if(context.t.Type == CoralParser.Text)
            {
                r.data = "string";
                r.text = context.Text().GetText();
            }
            else if(context.t.Type == CoralParser.True)
            {
                r.data = "bool";
                r.text = context.True().GetText();
            }
            else if(context.t.Type == CoralParser.False)
            {
                r.data = "bool";
                r.text = context.False().GetText();
            }
            else if(context.t.Type == CoralParser.Nil)
            {
                r.data = "object";
                r.text = "null";
            }
            return r;
        }

        public override object VisitLambda([NotNull] CoralParser.LambdaContext context)
        {
            var r = new Result();
            r.data = "var";
            r.text += "(" + Visit(context.lambdaIn()) + ")";
            r.text += "=>";
            r.text += "{" + Visit(context.lambdaOut()) + "}";
            return r;
        }

        public override object VisitLambdaIn([NotNull] CoralParser.LambdaInContext context)
        {
            var obj = "";
            for(int i = 0; i < context.id().Length; i++)
            {
                var r = (Result)Visit(context.id(i));
                if(i == 0)
                {
                    obj += r.text;
                }
                else
                {
                    obj += ", " + r.text;
                }
            }
            return obj;
        }

        public override object VisitLambdaOut([NotNull] CoralParser.LambdaOutContext context)
        {
            var obj = "";
            foreach(var item in context.functionSupportStatement())
            {
                obj += Visit(item);
            }
            return obj;
        }

        List<string> keywords = new List<string> {
        "abstract", "as", "base", "bool", "break" , "byte", "case" , "catch",
        "char","checked","class","const","continue","decimal","default","delegate","do","double","else",
        "enum","event","explicit","extern","false","finally","fixed","float","for","foreach","goto",
        "if","implicit","in","int","interface","internal","is","lock","long","namespace","new","null",
        "object","operator","out","override","params","private","protected","public","readonly","ref",
        "return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch",
        "this","throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using",
        "virtual","void","volatile","while"
        };
    }
}


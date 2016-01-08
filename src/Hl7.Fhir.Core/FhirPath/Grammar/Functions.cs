﻿using Hl7.Fhir.Support;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.FhirPath.Grammar
{
    internal static class Functions
    {
        internal static Parser<IEnumerable<Evaluator>> createFunctionParser(string name)
        {
            return
                from n in Parse.String(name).Token()
                from lparen in Parse.Char('(')
                from paramList in Parse.Ref(() => Expression.Expr.Named("parameter")).DelimitedBy(Parse.Char(',').Token()).Optional()
                from rparen in Parse.Char(')')
                select paramList.GetOrElse(Enumerable.Empty<Evaluator>());
        }

        internal static Parser<Evaluator> CreateFunctionParser(string name, Func<Evaluator> func)
        {
            return createFunctionParser(name).Select(p => invoke(func, p, name));
        }

        internal static Parser<Evaluator> CreateFunctionParser(string name, string paramName, Func<Evaluator,Evaluator> func)
        {
            return createFunctionParser(name).Select(p => invoke(func, p, name, paramName));
        }


        internal static Evaluator invoke(Func<Evaluator> func, IEnumerable<Evaluator> paramList, string name)
        {
            if (!paramList.Any())
                return func();
            else
                throw Error.Argument("Function '{0}' takes no parameters".FormatWith(name));
        }

        private static Evaluator invoke(Func<Evaluator, Evaluator> func, IEnumerable<Evaluator> paramList, string name, string paramName)
        {
            if (paramList.Count() == 1)
            {
                return func(paramList.Single());
            }
            else
                throw Error.Argument("Function '{0}' takes exactly one parameter '{1}'".FormatWith(name, paramName));
        }

        public static readonly Parser<Evaluator> Not = CreateFunctionParser("not", Eval.Not);
        public static readonly Parser<Evaluator> Empty = CreateFunctionParser("empty", Eval.Empty);
        public static readonly Parser<Evaluator> Where = CreateFunctionParser("where", "criterium", Eval.Where);
        public static readonly Parser<Evaluator> All = CreateFunctionParser("all", "criterium", Eval.All);
        public static readonly Parser<Evaluator> Any = CreateFunctionParser("any", "criterium", Eval.Any);
        public static readonly Parser<Evaluator> Item = CreateFunctionParser("item", "index", Eval.Item);
        public static readonly Parser<Evaluator> First = CreateFunctionParser("first", Eval.First);
        public static readonly Parser<Evaluator> Last = CreateFunctionParser("last", Eval.Last);
        public static readonly Parser<Evaluator> Tail = CreateFunctionParser("tail", Eval.Tail);
        public static readonly Parser<Evaluator> Skip = CreateFunctionParser("skip", "num", Eval.Skip);
        public static readonly Parser<Evaluator> Take = CreateFunctionParser("take", "num", Eval.Take);
        public static readonly Parser<Evaluator> Count = CreateFunctionParser("count", Eval.Count);
        public static readonly Parser<Evaluator> AsInteger = CreateFunctionParser("asInteger", Eval.AsInteger);
        public static readonly Parser<Evaluator> StartsWith = CreateFunctionParser("startsWith", "prefix", Eval.StartsWith);
        public static readonly Parser<Evaluator> Log = CreateFunctionParser("log", "argument", Eval.Log);
        public static readonly Parser<Evaluator> Resolve = CreateFunctionParser("resolve", Eval.Resolve);
        public static readonly Parser<Evaluator> Length = CreateFunctionParser("length", Eval.Length);
        public static readonly Parser<Evaluator> Distinct = CreateFunctionParser("distinct", Eval.Distinct);

        // function: ID '(' param_list? ')';
        // param_list: expr(',' expr)*;
        public static readonly Parser<Evaluator> OtherFunction =
            from name in Lexer.Id.Token()
            from lparen in Parse.Char('(')
            from paramList in Parse.Ref(() => Expression.Expr.Named("parameter")).DelimitedBy(Parse.Char(',').Token()).Optional()
            from rparen in Parse.Char(')')
            select Eval.Function(name, paramList.GetOrElse(Enumerable.Empty<Evaluator>()));


        public static readonly Parser<Evaluator> Function = Not.Or(Empty).Or(Where).Or(All).Or(Any).Or(Item)
                        .Or(First).Or(Last).Or(Tail).Or(Skip).Or(Take).Or(Count).Or(AsInteger).Or(StartsWith)
                        .Or(Log).Or(Resolve).Or(Length).Or(Distinct)
                        .Or(OtherFunction);
    }
}

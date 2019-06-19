"Compiler" {
    "Antlr4/Runtime"
    "Antlr4/Runtime/Misc"
    "System"

    "Compiler" XsParser.
    "Compiler" Compiler Static.
}

Iterator -> {
    begin(): Result
    end(): Result
    step(): Result
    order():Str = T
    attach():Str = F
}

(me:XsLangVisitor)(base) VisitIteratorStatement(context: IteratorStatementContext) -> (v: {}) {
    it := Iterator{}
    ? context.op.Text == ">=" | context.op.Text == "<=" {
        it.attach = T
    }
    ? context.op.Text == ">" | context.op.Text == ">=" {
        it.order = F
    }
    ? context.expression().Length == 2 {
        it.begin = Visit(context.expression(0)):(Result)
        it.end = Visit(context.expression(1)):(Result)
        it.step = Result{ data = I32, text = "1" }
    } _ {
        it.begin = Visit(context.expression(0)):(Result)
        it.end = Visit(context.expression(1)):(Result)
        it.step = Visit(context.expression(2)):(Result)
    }
    <- (it)
}

(me:XsLangVisitor)(base) VisitLoopStatement(context: LoopStatementContext) -> (v: {}) {
    obj := ""
    id := "ea"
    ? context.id() >< () {
        id = Visit(context.id()):(Result).text
    }
    it := Visit(context.iteratorStatement()):(Iterator)

    obj += "foreach (var "id" in Range("it.begin.text","it.end.text","it.step.text","it.order","it.attach"))"

    obj += ""BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight" "Terminate+Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitLoopInfiniteStatement(context: LoopInfiniteStatementContext) -> (v: {}) {
    obj := "for (;;) "BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight" "Terminate+Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitLoopEachStatement(context: LoopEachStatementContext) -> (v: {}) {
    obj := ""
    arr := Visit(context.expression()):(Result)
    target := arr.text
    id := "ea"
    ? context.id().Length == 2 {
        target = "Range("target")"
        id = "("Visit(context.id(0)):(Result).text","Visit(context.id(1)):(Result).text")"
    } context.id().Length == 1 {
        id = Visit(context.id(0)):(Result).text
    }

    obj += "foreach (var "id" in "target")"
    obj += ""BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight" "Terminate+Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitLoopCaseStatement(context: LoopCaseStatementContext) -> (v: {}) {
    obj := ""
    expr := Visit(context.expression()):(Result)
    obj += "for ( ;"expr.text" ;)"
    obj += ""BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight" "Terminate+Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitLoopJumpStatement(context: LoopJumpStatementContext) -> (v: {}) {
    <- ("break "Terminate+Wrap"")
}

(me:XsLangVisitor)(base) VisitLoopContinueStatement(context: LoopContinueStatementContext) -> (v: {}) {
    <- ("continue "Terminate+Wrap"")
}

(me:XsLangVisitor)(base) VisitJudgeCaseStatement(context: JudgeCaseStatementContext) -> (v: {})  {
    obj := ""
    expr := Visit(context.expression()):(Result)
    obj += "switch ("expr.text") "BlockLeft+Wrap""
    context.caseStatement() @ item {
        r := Visit(item):(Str)
        obj += r + Wrap
    }
    obj += ""BlockRight" "Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitCaseDefaultStatement(context: CaseDefaultStatementContext) -> (v: {}) {
    obj := ""
    obj += "default:"BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight"break;"
    <- (obj)
}

(me:XsLangVisitor)(base) VisitCaseExprStatement(context: CaseExprStatementContext) -> (v: {}) {
    obj := ""
    ? context.typeType() == () {
        expr := Visit(context.expression()):(Result)
        obj += "case "expr.text" :"Wrap""
    } _ {
        id := "it"
        ? context.id() >< () {
            id = Visit(context.id()):(Result).text
        }
        type := Visit(context.typeType()):(Str)
        obj += "case "type" "id" :"Wrap""
    }

    obj += ""BlockLeft" "ProcessFunctionSupport(context.functionSupportStatement())"" BlockRight" "
    obj += "break;"
    <- (obj)
}

(me:XsLangVisitor)(base) VisitCaseStatement(context: CaseStatementContext) -> (v: {}) {
    obj := Visit(context.GetChild(0)):(Str)
    <- (obj)
}

(me:XsLangVisitor)(base) VisitJudgeStatement(context: JudgeStatementContext) -> (v: {}) {
    obj := ""
    obj += Visit(context.judgeIfStatement())
    context.judgeElseIfStatement() @ it {
        obj += Visit(it)
    }
    ? context.judgeElseStatement() >< () {
        obj += Visit(context.judgeElseStatement())
    }
    <- (obj)
}

(me:XsLangVisitor)(base) VisitJudgeIfStatement(context: JudgeIfStatementContext) -> (v: {}) {
    b := Visit(context.expression()):(Result)
    obj := "if ( "b.text" ) "BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight""Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitJudgeElseIfStatement(context: JudgeElseIfStatementContext) -> (v: {}) {
    b := Visit(context.expression()):(Result)
    obj := "else if ( "b.text" ) "BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight" "Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitJudgeElseStatement(context: JudgeElseStatementContext) -> (v: {}) {
    obj := "else "BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight""Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitCheckStatement(context: CheckStatementContext) -> (v: {}) {
    obj := "try "BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight+Wrap""
    context.checkErrorStatement() @ item {
        obj += ""Visit(item)"" Wrap""
    }

    ? context.checkFinallyStatment() >< () {
        obj += Visit(context.checkFinallyStatment())
    }
    <- (obj)
}

(me:XsLangVisitor)(base) VisitCheckErrorStatement(context: CheckErrorStatementContext) -> (v: {}) {
    obj := ""
    ID := "ex"
    ? context.id() >< () {
        ID = Visit(context.id()):(Result).text
    }

    Type := "Exception"
    ? context.typeType() >< () {
        Type = Visit(context.typeType()):(Str)
    }

    obj += "catch( "Type" "ID" )"+Wrap+BlockLeft+Wrap
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += BlockRight
    <- (obj)
}

(me:XsLangVisitor)(base) VisitCheckFinallyStatment(context: CheckFinallyStatmentContext) -> (v: {}) {
    obj := "finally "Wrap+BlockLeft+Wrap""
    obj += ProcessFunctionSupport(context.functionSupportStatement())
    obj += ""BlockRight""Wrap""
    <- (obj)
}

(me:XsLangVisitor)(base) VisitUsingStatement(context: UsingStatementContext) -> (v: {}) {
    obj := ""
    r2 := Visit(context.expression(0)):(Result)
    r1 := Visit(context.expression(1)):(Result)
    ? context.typeType() >< () {
        Type := Visit(context.typeType()):(Str)
        obj = ""Type" "r1.text" = "r2.text""
    } _ {
        obj = "var "r1.text" = "r2.text""
    }
    <- (obj)
}

(me:XsLangVisitor)(base) VisitReportStatement(context: ReportStatementContext) -> (v: {}) {
    obj := ""
    ? context.expression() >< () {
        r := Visit(context.expression()):(Result)
        obj += r.text
    }
    <- ("throw "obj+Terminate+Wrap"")
}

(me:XsLangVisitor)(base) VisitLinq(context: LinqContext) -> (v: {}) {
    r := Result{data = "var"}
    r.text += "from " Visit(context.expression(0)):(Result).text " "
    context.linqItem() @ item {
        r.text += "" Visit(item) " "
    }
    r.text += ""context.k.Text " " Visit(context.expression(1)):(Result).text ""
    <- (r)
}

(me:XsLangVisitor)(base) VisitLinqItem(context: LinqItemContext) -> (v: {}) {
    obj := Visit(context.linqKeyword()):(Str)
    ? context.expression() >< () {
        obj += " "Visit(context.expression()):(Result).text""
    }
    <- (obj)
}

(me:XsLangVisitor)(base) VisitLinqKeyword(context: LinqKeywordContext) -> (v: {}) {
    <- (Visit(context.GetChild(0)))
}

(me:XsLangVisitor)(base) VisitLinqHeadKeyword(context: LinqHeadKeywordContext) -> (v: {}) {
    <- (context.k.Text)
}

(me:XsLangVisitor)(base) VisitLinqBodyKeyword(context: LinqBodyKeywordContext) -> (v: {}) {
    <- (context.k.Text)
}

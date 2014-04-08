options{
    STATIC=false ;
    IGNORE_CASE=true;
	VISIBILITY_INTERNAL = true;
}

PARSER_BEGIN(PlSql)
namespace Deveel.Data.Sql.Parser;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Deveel.Data.Expressions;
using Deveel.Data.Sql.Statements;

class PlSql {
    private String lastObjectReference = null;

    protected bool seeTYPE() {
        return "TYPE".Equals(GetToken(1).image, StringComparison.OrdinalIgnoreCase);
    }

    protected static IList<String> ANALYTIC_FUNCTION_NAMES = new List<String>();

    protected bool seeAnalyticFunction() {
        return "(".Equals(GetToken(2).image)
            && ANALYTIC_FUNCTION_NAMES.Contains(GetToken(1).image.ToUpper());
    }

	public void Reset() {
	}
}
PARSER_END(PlSql)


SKIP:
{
    " "
|   "\t"
|   "\r"
|   "\n"
}

/* Prefix      Meaning
    -------------------
    K_          Keyword
    O_          Operator
    S_          Substitutes
*/

TOKEN: /* SQL and PLSQL Keywords. prefixed with K_ to avoid name clashes */
{
    <K_ALL: "ALL">
|   <K_ALTER: "ALTER">
|   <K_AND: "AND">
|   <K_ANY: "ANY">
|   <K_AS: "AS">
|   <K_ASC:"ASC">
|   <K_BEGIN: "BEGIN">
|   <K_BETWEEN:"BETWEEN">
|   <K_BINARY_INTEGER: "BINARY_INTEGER">
|   <K_BOOLEAN:"BOOLEAN">
|   <K_BY:"BY">
|   <K_CALL:"CALL">
|   <K_CASE:"CASE">
|   <K_CAST:"CAST">
|   <K_CHAR:"CHAR">
|   <K_CLOSE:"CLOSE">
|   <K_COMMENT:"COMMENT">
|   <K_COMMIT:"COMMIT">
|   <K_COMMITTED:"COMMITTED">
|   <K_CONNECT:"CONNECT">
|   <K_CONSTANT:"CONSTANT">
|   <K_CONSTRAINT:"CONSTRAINT">
|   <K_CONSTRAINTS:"CONSTRAINTS">
|   <K_CURRENT:"CURRENT">
|   <K_CURSOR:"CURSOR">
|   <K_DATE:"DATE">
|   <K_DECIMAL:"DECIMAL">
|   <K_DECLARE:"DECLARE">
|   <K_DEFAULT:"DEFAULT">
|   <K_DELETE:"DELETE">
|   <K_DESC:"DESC">
|   <K_DISTINCT:"DISTINCT">
|   <K_DO:"DO">
|   <K_ELSE:"ELSE">
|   <K_ELSIF:"ELSIF">
|   <K_END:"END">
|   <K_ESCAPE:"ESCAPE">
|   <K_EXCEPTION:"EXCEPTION">
|   <K_EXCEPTION_INIT:"EXCEPTION_INIT">
|   <K_EXCLUSIVE:"EXCLUSIVE">
|   <K_EXISTS:"EXISTS">
|   <K_EXIT:"EXIT">
|   <K_FETCH:"FETCH">
|   <K_FLOAT:"FLOAT">
|   <K_FOR:"FOR">
|   <K_FORALL:"FORALL">
|   <K_FROM:"FROM">
|   <K_FULL:"FULL">
|   <K_FUNCTION:"FUNCTION">
|   <K_GOTO:"GOTO">
|   <K_GROUP:"GROUP">
|   <K_HAVING:"HAVING">
|   <K_IF:"IF">
|   <K_IN:"IN">
|   <K_INDEX:"INDEX">
|   <K_INNER:"INNER">
|   <K_INSERT:"INSERT">
|   <K_INTEGER:"INTEGER">
|   <K_INTERSECT:"INTERSECT">
|   <K_INTO:"INTO">
|   <K_IS:"IS">
|   <K_ISOLATION:"ISOLATION">
|   <K_LEFT:"LEFT">
|   <K_LIKE:"LIKE">
|   <K_LOCK:"LOCK">
|   <K_LOOP:"LOOP">
|   <K_MERGE:"MERGE">
|   <K_MINUS:"MINUS">
|   <K_NATURAL:"NATURAL">
|   <K_NOT:"NOT">
|   <K_NOWAIT:"NOWAIT">
|   <K_NULL:"NULL">
|   <K_NULLS:"NULLS">
|   <K_NUMBER:"NUMBER">
|   <K_OF:"OF">
|   <K_ON:"ON">
|   <K_ONLY:"ONLY">
|   <K_OPEN:"OPEN">
|   <K_OR:"OR">
|   <K_ORDER:"ORDER">
|   <K_OUT:"OUT">
|   <K_OVER:"OVER">
|   <K_PACKAGE:"PACKAGE">
|   <K_PARTITION:"PARTITION">
|   <K_POSITIVE:"POSITIVE">
|   <K_PRAGMA:"PRAGMA">
|   <K_PRIOR:"PRIOR">
|   <K_PROCEDURE:"PROCEDURE">
|   <K_PX_GRANULE:"PX_GRANULE">
|   <K_RAISE:"RAISE">
|   <K_RANGE:"RANGE">
|   <K_READ:"READ">
|   <K_REAL:"REAL">
|   <K_RECORD:"RECORD">
|   <K_REF:"REF">
|   <K_RETURN:"RETURN">
|   <K_RETURNING:"RETURNING">
|   <K_REVERSE:"REVERSE">
|   <K_RIGHT:"RIGHT">
|   <K_ROLLBACK:"ROLLBACK">
|   <K_ROW:"ROW">
|   <K_ROWS:"ROWS">
|   <K_SAMPLE:"SAMPLE">
|   <K_SAVEPOINT:"SAVEPOINT">
|   <K_SELECT:"SELECT">
|   <K_SERIALIZABLE:"SERIALIZABLE">
|   <K_SET:"SET">
|   <K_SHARE:"SHARE">
|   <K_SIBLINGS:"SIBLINGS">
|   <K_SKIP:"SKIP">
|   <K_SMALLINT:"SMALLINT">
|   <K_SQL:"SQL">
|   <K_START:"START">
|   <K_TABLE:"TABLE">
|   <K_TEST:"TEST">
|   <K_THEN:"THEN">
|   <K_TO:"TO">
|   <K_TRANSACTION:"TRANSACTION">
|   <K_UNION:"UNION">
|   <K_UNIQUE:"UNIQUE">
|   <K_UPDATE:"UPDATE">
|   <K_USE:"USE">
|   <K_USING:"USING">
|   <K_VALUES:"VALUES">
|   <K_VARCHAR2:"VARCHAR2">
|   <K_VARCHAR:"VARCHAR">
|   <K_WAIT:"WAIT">
|   <K_WHEN:"WHEN">
|   <K_WHERE:"WHERE">
|   <K_WHILE:"WHILE">
|   <K_WITH:"WITH">
|   <K_WORK:"WORK">
|   <K_WRITE:"WRITE">
}

TOKEN : /* Numeric Constants */
{
	< S_NUMBER: <FLOAT>
	    | <FLOAT> ( ["e","E"] ([ "-","+"])? <FLOAT> )?
    	>
  | 	< #FLOAT: <INTEGER>
	    | <INTEGER> ( "." <INTEGER> )?
	    | "." <INTEGER>
    	>
  | 	< #INTEGER: ( <DIGIT> )+ >
  | 	< #DIGIT: ["0" - "9"] >
}

SPECIAL_TOKEN:
{
   <LINE_COMMENT: "--"(~["\r","\n"])*>
|  <MULTI_LINE_COMMENT: "/*" (~["*"])* "*" ("*" | (~["*","/"] (~["*"])* "*"))* "/">
}


TOKEN:
{
    < S_IDENTIFIER: (<LETTER>)+ (<DIGIT> | <LETTER> |<SPECIAL_CHARS>)* >
  | < #LETTER: ["a"-"z", "A"-"Z"] >
  | < #SPECIAL_CHARS: "$" | "_" | "#">
  | < S_BIND: ":" ( <S_NUMBER> | <S_IDENTIFIER> ("." <S_IDENTIFIER>)?) >
  | < S_CHAR_LITERAL: "'" (~["'"])* "'" ("'" (~["'"])* "'")*>
  | < S_QUOTED_IDENTIFIER: "\"" (~["\n","\r","\""])* "\"" >
}

/* Represents a PLSQL code block. */
void CompilationUnit():
{}
{
    ProcedureDeclaration()
  | FunctionDeclaration()
  | SequenceOfStatements()
  | AlterSession()
  | "CALL" ProcedureCall()
}

void BindVariable():
{}
{
    <S_BIND>
  | ":" (<S_NUMBER> | <S_IDENTIFIER> ["." <S_IDENTIFIER>])
}

void AlterSession():
{}
{
    "ALTER" ID("SESSION") "SET"
    (<S_IDENTIFIER> "=" (<S_CHAR_LITERAL> | LOOKAHEAD({Regex.IsMatch(GetToken(1).image, "(?i)TRUE|FALSE")}) <S_IDENTIFIER>))*
    ["COMMENT" "=" <S_CHAR_LITERAL>]
}

void DeclarationSection():
{}
{
    "DECLARE" Declarations()
}

void Declarations():
{}
{
    ( ( LOOKAHEAD({seeTYPE()}) ID("TYPE") <S_IDENTIFIER> "IS" TypeDefinition()
      | CursorDeclaration()
      | PragmaDeclaration()
      | IdentifierDeclaration()
      )
      ";"
    )+
    ( LOOKAHEAD({!seeTYPE()}) ProcedureDeclaration()
    | LOOKAHEAD({!seeTYPE()}) FunctionDeclaration()
    )*
}

void IdentifierDeclaration():
{}
{
    <S_IDENTIFIER>

    ( ConstantDeclaration()
      |
      VariableDeclaration()
      |
      ExceptionDeclaration()
    )
}

void CursorDeclaration():
{}
{
   "CURSOR" <S_IDENTIFIER> ["(" ParameterList() ")" ]
        "IS" SelectStatement()
}

void PragmaDeclaration():
{}
{
    "PRAGMA" "EXCEPTION_INIT" "(" NumOrID() "," NumOrID() ")"
}

void ProcedureDeclaration():
{}
{
    "PROCEDURE" <S_IDENTIFIER> [ "(" ParameterList() ")" ]
    ( ";"  // Procedure Specification
      |
      "IS" ProcedureBody()
    )
}

void ProcedureBody():
{}
{
    [ Declarations() ]
    BeginEndBlock()
}

void FunctionDeclaration():
{}
{
    "FUNCTION" <S_IDENTIFIER> [ "(" ParameterList() ")" ]
    "RETURN" TypeDefinition()
    ( ";" // FunctionSpecification
      |
      "IS" FunctionBody()
    )
}

void FunctionBody():
{}
{
    [ Declarations() ]
    BeginEndBlock()
}

void VariableDeclaration():
{}
{
    TypeDefinition() [ "NOT" "NULL" ]
                        [ (":=" | "DEFAULT" ) PlSqlExpression() ]
}

void ConstantDeclaration():
{}
{
    "CONSTANT" TypeDefinition() [ "NOT" "NULL" ]
                        (":=" | "DEFAULT" ) PlSqlExpression()
}

void TypeDefinition():
{}
{
    BasicDataTypeDefinition()
    |
    "TABLE" "OF" TypeDefinition() [LOOKAHEAD(2) "INDEX" "BY" BasicDataTypeDefinition()]
    |
    LOOKAHEAD(2) (<S_IDENTIFIER> ("%TYPE" | "%ROWTYPE"))
    |
    LOOKAHEAD(TableColumn() "%TYPE")TableColumn()"%TYPE"
    |
    <S_IDENTIFIER>  // For Alert declarations etc, in Forms
}



void BasicDataTypeDefinition():
{}
{
    (       "CHAR"
        |   "VARCHAR"
        |   "VARCHAR2"
        |   "INTEGER"
        |   "NUMBER"
        |   "NATURAL"
        |   "REAL"
        |   "FLOAT"
    ) [ "(" <S_NUMBER> [ "," <S_NUMBER> ] ")" ]

    |   "DATE"
    |   "BINARY_INTEGER"
    |   "BOOLEAN"
}


void ExceptionDeclaration():
{}
{
    "EXCEPTION"
}

/* ---------------- DECLARATIONS SECTION ends here ------------------ */

/* ---------------- Code Section starts here ---------------------- */
                                
void BeginEndBlock():
{}
{
    "BEGIN"
    SequenceOfStatements()
    [ ExceptionBlock()]
    "END" [<S_IDENTIFIER>] ";"
}

IEnumerable<Statement> SequenceOfStatements():
{ var statements = new List<Statement>();
  Statement statement; }
{
    ( statement = PLSQLStatement() { statements.Add(statement); } )+
	{ return statements.AsReadOnly(); }
}

void ExceptionBlock():
{}
{
    "EXCEPTION"
    (ExceptionHandler())+
}

void ExceptionHandler():
{}
{
    "WHEN" ( <S_IDENTIFIER> ("OR" <S_IDENTIFIER>)*
             // "OTHERS" is treated as an identifier.
             // Making "OTHERS" a keyword causes problems; for example,
             // a reference to a column named "OTHERS" won't be parsed.
           )
    "THEN" SequenceOfStatements()
}

Statement PLSQLStatement():
{ Statement statement = null; }
{
	(
    ExitStatement()
    |
    GotoStatement()
    |
    IfStatement()
    |
    LabelDeclaration()
    |
    LoopStatement()
    |
    NullStatement()
    |
    RaiseStatement()
    |
    ReturnStatement()
    |
    ForallStatement()
    |
    statement = SQLStatement()
    |
    [DeclarationSection()] BeginEndBlock()
    |
    LOOKAHEAD(DataItem() ":=") AssignmentStatement()
    |
    LOOKAHEAD(ProcedureCall()) ProcedureCall()
	)
	{ return statement; }
}

void LabelDeclaration():
{}
{
    "<<" <S_IDENTIFIER> ">>"
}

void ForallStatement():
{}
{
    "FORALL" <S_IDENTIFIER> "IN" PlSqlSimpleExpression() ".." PlSqlSimpleExpression()
    ( InsertStatement() | UpdateStatement() | DeleteStatement())
}

Statement SQLStatement():
{ Statement statement = null; }
{
	(
    CloseStatement()
    |
    CommitStatement()
    |
    DeleteStatement()
    |
    FetchStatement()
    |
    InsertStatement()
    |
    LockTableStatement()
    |
    OpenStatement()
    |
    RollbackStatement()
    |
    SavepointStatement()
    |
    statement = QueryStatement()
    |
    SetStatement()
    |
    UpdateStatement()
	/*
    |
    MergeStatement()
	*/
	)
	{ return statement; }
}

void ProcedureCall():
{}
{
    ProcedureReference() [ "(" [ Arguments() ] ")" ] ";"
}

string ProcedureReference():
{
    String name;
}
{
    name = ObjectReference()
    {return name; }
}

void AssignmentStatement():
{}
{
    DataItem() ":=" PlSqlExpression() ";"
}


void ExitStatement():
{}
{
    "EXIT" [ <S_IDENTIFIER>] ["WHEN" PlSqlExpression()] ";"
}

void GotoStatement():
{}
{
    "GOTO" <S_IDENTIFIER> ";"
}

void IfStatement():
{}
{
    "IF" PlSqlExpression()
    "THEN"
          SequenceOfStatements()
    ("ELSIF" PlSqlExpression()
     "THEN"
             SequenceOfStatements()
    )*
    ["ELSE"
            SequenceOfStatements()
    ]
    "END" "IF" [<S_IDENTIFIER>] ";"
}

void LoopStatement():
{}
{
    NormalLoop()
    |
    WhileLoop()
    |
    ForLoop()
}

void NormalLoop():
{}
{
    "LOOP"
        SequenceOfStatements()
    "END" "LOOP" [<S_IDENTIFIER>] ";"
}

void WhileLoop():
{}
{
    "WHILE"  PlSqlExpression()
     NormalLoop()
}

void ForLoop():
{}
{
    LOOKAHEAD(NumericForLoopLookahead())
    NumericForLoop()
    |
    CursorForLoop()
}

void NumericForLoopLookahead():
{}
{
    "FOR" <S_IDENTIFIER> "IN" ["REVERSE"]
          PlSqlSimpleExpression() ".."
}

void NumericForLoop():
{}
{
    "FOR" <S_IDENTIFIER> "IN" ["REVERSE"]
          PlSqlSimpleExpression() ".." PlSqlSimpleExpression()
    NormalLoop()

}

void CursorForLoop():
{}
{
  "FOR" <S_IDENTIFIER> "IN" ( CursorReference() [ "(" Arguments() ")"]
                              | "(" SelectStatement() ")"
                            )
  NormalLoop()
}

void CursorReference():
{}
{
    ObjectReference()
}

void NullStatement():
{}
{
    "NULL" ";"
}

void RaiseStatement():
{}
{
    "RAISE" [<S_IDENTIFIER>] ";"
}


void ReturnStatement():
{}
{
    "RETURN" [ PlSqlExpression() ] ";"
}


void CloseStatement():
{}
{
    "CLOSE" <S_IDENTIFIER> ";"
}

void CommitStatement():
{}
{
    "COMMIT" ["WORK"] ["COMMENT" <S_CHAR_LITERAL>] ";"
}

void FetchStatement():
{}
{
    "FETCH" (<S_IDENTIFIER>)
    ( "INTO" (<S_IDENTIFIER> | BindVariable()) ("," (<S_IDENTIFIER> | BindVariable()))*
    | LOOKAHEAD(3) ID("BULK") ID("COLLECT") "INTO"
        (<S_IDENTIFIER> | ":" <S_IDENTIFIER>) ("," (<S_IDENTIFIER> | ":" <S_IDENTIFIER>))*
        [LOOKAHEAD(2) ID("LIMIT") PlSqlSimpleExpression()]
    ) ";"
}

void LockTableStatement():
{}
{
    "LOCK" "TABLE" TableName() ("," TableName())*
    "IN" LockMode() ID("MODE") ["NOWAIT"] ";"
}

void OpenStatement():
{}
{
    "OPEN" CursorReference() ["(" Arguments() ")"] ";"
}

void RollbackStatement():
{}
{
    "ROLLBACK" ["WORK"] ["TO" ["SAVEPOINT"] <S_IDENTIFIER>]
    ["COMMENT" <S_CHAR_LITERAL>] ";"
}

void SetStatement():
{}
{
    "SET"
      ("TRANSACTION" (  "READ" ("ONLY" | "WRITE")
                      | "ISOLATION" ID("LEVEL") ("SERIALIZABLE" | "READ" ID("COMMITTED"))
                      | "USE" "ROLLBACK" ID("SEGMENT") ObjectReference())
       | ("CONSTRAINT" | "CONSTRAINTS") ("ALL" | <S_IDENTIFIER> ("," <S_IDENTIFIER>)*)
         [LOOKAHEAD({Regex.IsMatch(GetToken(1).image, "(?i)IMMEDIATE|DEFERRED")}) <S_IDENTIFIER>]
      )
    ";"
}

void LockMode():
{}
{
    ("ROW" ("SHARE" | "EXCLUSIVE"))
  | ("SHARE" ["UPDATE" | ("ROW" "EXCLUSIVE")])
  | ("EXCLUSIVE")
}

void SavepointStatement():
{}
{
    "SAVEPOINT" <S_IDENTIFIER> ";"
}

void UpdateStatement():
{}
{
    "UPDATE" (TableName() | "(" SubQuery() ")") [OracleObjectName()]
    "SET" ColumnValues()
    ["WHERE" (SQLExpression() | "CURRENT" "OF" <S_IDENTIFIER>)]
    [ReturningClause()]
    ";"
}

void ReturningClause():
{}
{
    "RETURNING" SQLExpression() ("," SQLExpression())* IntoClause()
}

void ColumnValues():
{}
{
    ColumnValue() ("," ColumnValue())*
  | "(" TableColumn() ("," TableColumn())* ")" "=" "(" SelectStatement() ")"
}

void ColumnValue():
{}
{
    TableColumn() "=" PlSqlExpression()
}

void InsertStatement():
{}
{
    "INSERT" "INTO" TableName() [OracleObjectName()]
     [ LOOKAHEAD(2) "(" TableColumn() ("," TableColumn())* ")" ]
    ( "VALUES" "(" PlSqlExpressionList() ")" [ReturningClause()]
      | SubQuery()
    )
    ";"
}

/*
void MergeTableReference():
{}
{
    ( TableName() // might also be a query name
     | TableCollectionExpression()
     | LOOKAHEAD(3) "(" SubQuery() ")"
     | "(" TableReference() ")"
     | BindVariable() // not valid SQL, but appears in StatsPack SQL text
    )
    ["PX_GRANULE" "(" <S_NUMBER> "," <S_IDENTIFIER> "," <S_IDENTIFIER> ")"]
    ["SAMPLE" [ID("BLOCK")] "(" <S_NUMBER> ")"]
    [ OracleObjectName()] // alias

    (Join())*
}

void MergeStatement():
{}
{
    "MERGE" "INTO" MergeTableReference()
    "USING" MergeTableReference() "ON" "(" SQLExpression() ")"
    "WHEN" ID("MATCHED") "THEN"
        "UPDATE" "SET" MergeSetColumn() ("," MergeSetColumn())*
    "WHEN" "NOT" ID("MATCHED") "THEN"
        "INSERT" "(" TableColumn() ("," TableColumn())* ")"
        "VALUES" "(" ("DEFAULT" | SQLExpressionList()) ")"
    ";"
}

void MergeSetColumn():
{}
{
    TableColumn() "=" ("DEFAULT" | SQLExpression())
}
*/

void DeleteStatement():
{}
{
    "DELETE" ["FROM"] TableName() [OracleObjectName()]
    ["WHERE" (SQLExpression() | "CURRENT" "OF" <S_IDENTIFIER> ) ] ";"
}

Statement QueryStatement():
{ Statement statement; }
{
    statement = SelectStatement() ";"
	{ return statement; }
}

// PLSQL Expression and it's childs

Expression PlSqlExpression():
{ Expression exp = null, otherExp = null; }
{
    exp = PlSqlAndExpression() 
	("OR" otherExp = PlSqlAndExpression() { exp = Expression.Or(exp, otherExp); })*
	{ return exp; }
}

Expression PlSqlAndExpression():
{ Expression exp = null, otherExp = null; }
{
    exp = PlSqlUnaryLogicalExpression() 
	( "AND" otherExp = PlSqlUnaryLogicalExpression() { exp = Expression.And(exp, otherExp); } )*
	{ return exp; }
}

Expression PlSqlUnaryLogicalExpression():
{ Expression exp = null;
  bool isNot = false; }
{
  ["NOT" { isNot = true; } ] exp = PlSqlRelationalExpression()
  { if (isNot) exp = Expression.Not(exp);  }
  { return exp; }
}

Expression PlSqlRelationalExpression():
{ Expression exp = null, otherExp = null;
  Operator op; }
{
    exp = PlSqlSimpleExpression()

    ( op = Relop() otherExp = PlSqlSimpleExpression() { exp = ParserUtil.Binary(exp, op, otherExp); }
      |
      LOOKAHEAD(2) exp = PlSqlInClause(exp)
      |
      LOOKAHEAD(2) exp = PlSqlBetweenClause(exp)
      |
      LOOKAHEAD(2) exp = PlSqlLikeClause(exp)
      |
      exp = IsNullClause(exp)
   )?
   { return exp; }
}

IList<Expression> PlSqlExpressionList():
{ List<Expression> exps = new List<Expression>();
  Expression exp; }
{
    exp = PlSqlExpression() { exps.Add(exp); } 
	("," exp = PlSqlExpression() { exps.Add(exp); } )*
	{ return exps; }
}

Expression PlSqlInClause(Expression exp):
{  IList<Expression> expList;
  bool isNot = false; }
{
    ["NOT" { isNot = true; } ] "IN" "(" expList = PlSqlExpressionList()")"
	{ exp = Expression.In(exp, expList); 
	if (isNot) exp = Expression.Not(exp); 
	return exp; }
}

Expression PlSqlBetweenClause(Expression exp):
{ Expression min = null; Expression max = null;
  bool isNot=false;}
{
    ["NOT" { isNot = true; } ] "BETWEEN" min = PlSqlSimpleExpression() "AND" max = PlSqlSimpleExpression()
	{ exp = Expression.Between(exp, min, max); 
	 if (isNot) exp = Expression.Not(exp); 
	 return exp; }
}

Expression PlSqlLikeClause(Expression exp):
{ Expression likeExp = null, escapeExp = null; 
  bool isNot = false; }
{
    ["NOT" { isNot = true; } ] "LIKE" likeExp = PlSqlSimpleExpression() [ "ESCAPE" escapeExp = PlSqlSimpleExpression()]
	{ exp = Expression.Like(exp, likeExp, escapeExp); 
	if (isNot) exp = Expression.Not(exp); 
	return exp; }
}

Expression IsNullClause(Expression exp):
{ bool isNot = false; }
{
    "IS" ["NOT" { isNot = true; } ] "NULL"
	{ exp = Expression.IsNull(exp);
	  if (isNot) exp = Expression.Not(exp);
	  return exp; }
}


Expression PlSqlSimpleExpression():
{ Expression exp = null, otherExp = null;
  Operator op; }
{
    exp = PlSqlMultiplicativeExpression() 
	( ("+" { op = Operator.Add; } | 
	   "-" { op = Operator.Subtract; } | 
	   "||" { op = Operator.Concat; } ) 
	   otherExp = PlSqlMultiplicativeExpression() { exp = ParserUtil.Binary(exp, op, otherExp); })*
	{ return exp; }
}


Expression PlSqlMultiplicativeExpression():
{ Expression exp, otherExp = null;
  Operator op; }
{
    exp = PlSqlExponentExpression() 
	( LOOKAHEAD(1) 
	  ( "*" { op = Operator.Multiply; } | 
	    "/" { op = Operator.Divide; } | 
		ID("MOD") { op = Operator.Modulo; } ) 
		otherExp = PlSqlExponentExpression() { exp = ParserUtil.Binary(exp, op, otherExp); } )*
	{ return exp; }
}

Expression PlSqlExponentExpression():
{ Expression exp, otherExp = null;
  Operator op; }
{
    exp = PlSqlUnaryExpression() 
	( "**" { op = Operator.Exponent; } otherExp = PlSqlUnaryExpression() { exp = ParserUtil.Binary(exp, op, otherExp); } )*
	{ return exp; }
}

Expression PlSqlUnaryExpression():
{ Expression exp;
  bool negative = false; }
{
    (("+" | "-" { negative = true; } ) exp = PlSqlPrimaryExpression())
	{ if (negative) exp = Expression.Negative(exp); }
|
    exp = PlSqlPrimaryExpression()
	{ return exp; }
}


Expression PlSqlPrimaryExpression():
{ Expression exp = null, otherExp = null;
  string refName = null;
  TableSelectExpression selectExp = null;
  Token t; }
{
 (   t = <S_NUMBER> { exp = Expression.Constant(ParserUtil.Number(t.image));}
  | t = <S_CHAR_LITERAL> { exp = Expression.Constant(ParserUtil.Unquote(t.image)); }
  | "NULL" { exp = Expression.Constant(null); }
  | exp = SQLCaseExpression()
  | "(" (LOOKAHEAD(3) selectExp = Select() { exp = Expression.Subquery(selectExp); } | 
       exp = PlSqlExpression() { exp = Expression.Subset(exp); } ) ")"
  | BindVariable()
  | LOOKAHEAD(2) exp = SQLCastExpression()
  | LOOKAHEAD(IntervalExpression()) IntervalExpression()
  | LOOKAHEAD(2) (<S_IDENTIFIER> | "SQL") "%" ID("FOUND|NOTFOUND|ISOPEN|ROWCOUNT")
  | LOOKAHEAD(FunctionReference() "(") exp = FunctionCall()
  | refName = ObjectReference() { exp = Expression.Variable(refName); } // Might be a call to a parameter-less function.
  )
  { return exp; }
}

/* ----------------------- PLSQL Code Block Ends here -------------- */

/* ---------------- General Productions --------------------- */

string TableColumn():
{ string name; }
{
    name = ObjectReference()
	{ return name; }
}

String OracleObjectName():
{}
{
    <S_IDENTIFIER>        {return token.image;}
  | <S_QUOTED_IDENTIFIER> {String s = token.image; return s.Substring(1, s.Length - 2);}
}

String TNSName():
{
    StringBuilder name = new StringBuilder();
}
{
    <S_IDENTIFIER>      {name.Append(token.image);}
    ("." <S_IDENTIFIER> {name.Append(".").Append(token.image);} )*
    {return name.ToString();}
}

Operator Relop():
{ Operator op ; }
{
    ( "=" { op = Operator.Equals; }
  | "!" "=" { op = Operator.NotEquals; }
  | "#" { op = Operator.Like; }
  | LOOKAHEAD(2) ">" "=" { op = Operator.GreaterEquals; }
  | ">" { op = Operator.Greater; }
  | LOOKAHEAD(2) "<" ">" { op =Operator.NotEquals; }
  | LOOKAHEAD(2) "<" "=" { op = Operator.SmallerEquals; }
  | "<" { op = Operator.Smaller; })
  { return op; }
}

string TableName():
{
    String s;
    StringBuilder name = new StringBuilder();
}
{
    // schema.table@link
    s=OracleObjectName()       {name.Append(s);}
    [ "." s=OracleObjectName() {name.Append(".").Append(s);} ]
    [ "@" s=TNSName()          {name.Append("@").Append(s);} ]
    { return name.ToString();}
}

void ParameterList():
{}
{
    Parameter() ( "," Parameter() )*
}

void NumOrID():
{}
{
    <S_IDENTIFIER> | (["+" | "-"] <S_NUMBER>)
}

void Parameter():
{}
{
    <S_IDENTIFIER> [ ["IN"] ["OUT"] TypeDefinition()
                             [(":=" | "DEFAULT" ) PlSqlExpression()] ]
}

void Arguments():
{}
{
    Argument() ("," Argument())*
}

void Argument():
{}
{
    [LOOKAHEAD(2) <S_IDENTIFIER> "=>"] PlSqlExpression()
}

/* --------------- General Productions ends here --------------- */

/* ----------- SQL productions start here ----------------- */

Statement SelectStatement():
{ SelectStatement statement = new SelectStatement();
  TableSelectExpression tableSelect; }
{
    tableSelect = SelectWithoutOrder() 
	{ statement.SelectExpression = tableSelect; }
    [ OrderByClause(statement.OrderBy) ]
    [ ForUpdateClause() ]
    [ "SKIP" ID("LOCKED") ]

	{ return statement; }
}

TableSelectExpression SelectWithoutOrder():
{ TableSelectExpression tableSelect, compositeSelect;
  CompositeFunction composite = CompositeFunction.None;
  bool isUnionAll = false; }
{
    tableSelect = SelectSet() 
	(
	  (
	    ("UNION" ["ALL" { isUnionAll = true; } ] { composite = CompositeFunction.Union; } ) | 
		"INTERSECT" { composite = CompositeFunction.Intersect; } | 
		( "MINUS" | "EXCEPT" ) { composite = CompositeFunction.Except; }
	  ) 
	  compositeSelect = SelectSet()
	  { tableSelect.ChainComposite(compositeSelect, composite, isUnionAll); }
	)*

	{ return tableSelect; }
}

TableSelectExpression SelectSet():
{ TableSelectExpression tableSelect = null; }
{
    ( tableSelect = Select() | 
	"(" SubQuery() ")" )
	{ return tableSelect; }
}

TableSelectExpression Select():
{ TableSelectExpression selectExpr = new TableSelectExpression(); }
{
    "SELECT" [ "ALL" | "DISTINCT" { selectExpr.Distinct = true; } | "UNIQUE" ] 
	SelectList(selectExpr.Columns)
    [IntoClause()]
    FromClause(selectExpr.From)
    [ selectExpr.Where = WhereClause() ]
    [ ConnectClause() ]
    [ selectExpr.Having = HavingClause() GroupByClause(selectExpr.GroupBy) | 
	  GroupByClause(selectExpr.GroupBy) [ selectExpr.Having = HavingClause() ]]
	{ return selectExpr; }
}

/* Checks for whatever follows  SELECT */
void SelectList(ICollection<SelectColumn> columns):
{ SelectColumn column; }
{
    "*" { column = new SelectColumn(Expression.Constant("*")); columns.Add(column); } 
	  | column = SelectItem() { columns.Add(column); }
	("," column = SelectItem() { columns.Add(column); } )*
}

SelectColumn SelectItem():
{ Expression exp; string name =null, nameExt = null, alias = null; }
{
    (
        LOOKAHEAD(2) name = OracleObjectName()".*" { exp = Expression.Variable(name + ".*"); } // table.*
      | LOOKAHEAD(4) name = OracleObjectName()"." nameExt = OracleObjectName() ".*" { exp = Expression.Variable(name + "." + nameExt + ".*"); } // schema.table.*
      | exp = SQLSimpleExpression() // column name or expression
    )
    [ [ "AS" ] alias = SelectItemAlias()]
	{ return new SelectColumn(exp, alias); }
}

string SelectItemAlias():
{ string alias = null; Token t = null; }
{
    alias = OracleObjectName()
    // Some keywords are acceptable as aliases:
  | t = "RETURNING" { alias = t.image; } | t = "WHEN" { alias = t.image; }
  { return alias; }
}

void AnalyticFunction():
{}
{
    FunctionCall() [ "OVER" "(" AnalyticClause() ")" ]
}

void AnalyticClause():
{}
{
    [ QueryPartitionClause() ] [ OrderByClause(null) [ WindowingClause() ] ]
}

void QueryPartitionClause():
{}
{
    "PARTITION" "BY" SQLExpression() ( "," SQLExpression() )*
}

void WindowingClause():
{}
{
    ("ROWS" | "RANGE")
    ( "CURRENT" "ROW"
    | SQLSimpleExpression() ID("PRECEDING") // perhaps UNBOUNDED PRECEDING
    | "BETWEEN" ( "CURRENT" "ROW"
                | SQLSimpleExpression() ID("PRECEDING|FOLLOWING") // perhaps UNBOUNDED FOLLOWING
                )
          "AND" ( "CURRENT" "ROW"
                | SQLSimpleExpression() ID("PRECEDING|FOLLOWING") // perhaps UNBOUNDED PRECEDING
                )
    )
}

void IntoClause():
{}
{
   "INTO" DataItem() ("," DataItem())*
}

void DataItem():
{}
{
    (<S_IDENTIFIER> ["." <S_IDENTIFIER>] | BindVariable())
    [ "(" PlSqlSimpleExpression() ")" ] // collection subscript
}

void FromClause(FromClause fromClause):
{ }
{
    "FROM" TableReference(fromClause)
	( "," TableReference(fromClause) )*
}

void TableReference(FromClause fromClause):
{ }
{
    "ONLY" "(" QueryTableExpression(fromClause) ")"
  | QueryTableExpression(fromClause)
}

void QueryTableExpression(FromClause fromClause):
{ }
{
    TableDeclaration(fromClause)
    (Join(fromClause))*
}

void TableDeclaration(FromClause fromClause):
{ string name = null, alias = null; TableSelectExpression selectExp = null;}
{
    ( name = TableName() // might also be a query name
     | TableCollectionExpression()
     | LOOKAHEAD(3) "("  SubQuery() ")"
     | "(" TableReference(fromClause) ")"
     | BindVariable() // not valid SQL, but appears in StatsPack SQL text
    )
    ["PX_GRANULE" "(" <S_NUMBER> "," <S_IDENTIFIER> "," <S_IDENTIFIER> ")"]
    ["SAMPLE" [ID("BLOCK")] "(" <S_NUMBER> ")"]
    [ alias = OracleObjectName()] // alias
	{ fromClause.AddTableDeclaration(name, selectExp, alias); }
}

void TableCollectionExpression():
{}
{
    "TABLE" "(" SQLSimpleExpression() ")" [ "(" "+" ")" ]
}

void Join(FromClause fromClause):
{ Expression onExpression; }
{
(
      (
        ","
        { fromClause.AddJoin(JoinType.Inner);}
      ) [ TableReference(fromClause) ]
    | (
        [ "INNER" ] ID("JOIN") TableDeclaration(fromClause) "ON" onExpression=SQLExpression()
        { fromClause.AddPreviousJoin(JoinType.Inner, onExpression); }
      ) [ Join(fromClause) ]
    | (
        "LEFT" ["OUTER"] ID("JOIN") TableDeclaration(fromClause) "ON" onExpression=SQLExpression()
        { fromClause.AddPreviousJoin(JoinType.Left, onExpression); }
      ) [ Join(fromClause) ]
    | (
        "RIGHT" ["OUTER"] ID("JOIN") TableDeclaration(fromClause) "ON" onExpression=SQLExpression()
        { fromClause.AddPreviousJoin(JoinType.Right, onExpression); }
      ) [ Join(fromClause) ]
  )

/*
    JoinType() ID("JOIN") TableReference() ("ON" onExpression = SQLExpression() | "USING" "(" ColumnName() ("," ColumnName())* ")")
//| ("CROSS" | "NATURAL" JoinType()) ID("JOIN") TableReference()
*/
}

/*
JoinType JoinType():
{ JoinType joinType = JoinType.Inner; }
{
    "INNER" { joinType = JoinType.Inner; }
  | ("LEFT" | "RIGHT" | "FULL") ID("OUTER")
  { return joinType; }
}
*/

void ColumnName():
{}
{
    <S_IDENTIFIER>
}

Expression WhereClause():
{ Expression exp; }
{
    "WHERE" exp = SQLExpression()
	{ return  exp; }
}

void ConnectClause():
{}
{
    // The following grammar will take 2 "START WITH" expressions
    // which is not correct. But alright, because only valid statements
    // will be given.
   (["START" "WITH" SQLExpression()] "CONNECT" "BY" SQLExpression()
    ["START" "WITH" SQLExpression()])
}

void GroupByClause(ICollection<ByColumn> columns):
{ IEnumerable<Expression> exps; }
{
    "GROUP" "BY" exps = SQLExpressionList() 
	{ 
		foreach(Expression exp in exps) {
			columns.Add(new ByColumn(exp));
		}
	}
}

Expression HavingClause():
{ Expression exp; }
{
    "HAVING" exp = SQLExpression()
	{ return (exp); }
}

void OrderByClause(ICollection<ByColumn> columns):
{ ByColumn column; }
{
    "ORDER" ["SIBLINGS"] "BY" column = OrderByExpression() { columns.Add(column); } 
	("," column = OrderByExpression() { columns.Add(column); } )*
}

ByColumn OrderByExpression():
{ Expression exp; bool ascending = false; }
{
    exp = SQLSimpleExpression()
    ["ASC" { ascending = true; } | "DESC"]
    ["NULLS" ID("LAST")]
	{ return new ByColumn(exp, ascending); }
}

void ForUpdateClause():
{}
{
    "FOR" "UPDATE" [ "OF" TableColumn() ("," TableColumn())* ]
    [ "NOWAIT" | "WAIT" SQLSimpleExpression() ]
}

Expression SQLExpression():
{ Expression exp, otherExp = null; }
{
    exp = SQLAndExpression() 
	("OR" otherExp = SQLAndExpression() { exp = Expression.Or(exp, otherExp); } )*
	{ return exp; }
}

Expression SQLAndExpression():
{ Expression exp, otherExp = null; }
{
    exp = SQLUnaryLogicalExpression() 
	( "AND" otherExp = SQLUnaryLogicalExpression() { exp = Expression.And(exp, otherExp); })*
	{ return exp; }
}

Expression SQLUnaryLogicalExpression():
{ Expression exp = null;
  bool isNot = false; }
{
  (
    LOOKAHEAD(2) ExistsClause()
  | ( ["NOT" { isNot = true; } ] exp = SQLRelationalExpression() { if (isNot) exp = Expression.Not(exp); })
  )
  { return exp; }
}

Expression ExistsClause():
{ Expression exp = null;
  bool isNot = false;
  TableSelectExpression query = null; }
{
    ["NOT" { isNot = true; } ] "EXISTS" "(" query = Select() ")"
	{ exp = Expression.FunctionCall("exists", ParserUtil.FunctionArgument(query));
	if (isNot)
	exp = Expression.Not(exp);
	return exp; }
}

Expression SQLRelationalExpression():
{ Expression exp = null; }
{
    /* Only after looking past "(", Expression() and "," we will know that
       it is expression list */
(
    (LOOKAHEAD("(" SQLSimpleExpression() ",")
     "(" SQLExpressionList() ")"
|
    (["PRIOR"] exp = SQLSimpleExpression()))

    /* Lookahead(2) is required because of NOT IN,NOT BETWEEN and NOT LIKE */
   ( exp = SQLRelationalOperatorExpression(exp) |  
     LOOKAHEAD(2) (SQLInClause()) |  
	 LOOKAHEAD(2) (SQLBetweenClause()) |  
	 LOOKAHEAD(2) (SQLLikeClause()) |  
	 IsNullClause(null)
   )?
   )
   { return exp; }
}

List<Expression> SQLExpressionList():
{ List<Expression> exps = new List<Expression>(); 
  Expression exp; }
{
    exp = SQLExpression() { exps.Add(exp); } 
	("," exp = SQLExpression() { exps.Add(exp); } )*
	{ return exps; }
}

Expression SQLRelationalOperatorExpression(Expression exp):
{ Expression exp1 = null; Operator op; }
{

    op = Relop()

    /* Only after seeing an ANY/ALL or "(" followed by a SubQuery() we can
    determine that is is a sub-query
    */
    (   LOOKAHEAD("ANY" | "ALL" | "(" "SELECT")
        (["ALL" | "ANY"] "(" SubQuery() ")")
        |
        ["PRIOR"] exp1 = SQLSimpleExpression()
    )
	{ return ParserUtil.Binary(exp, op, exp1); }
}

void SQLInClause():
{}
{
    ["NOT"] "IN" "(" (LOOKAHEAD(3) SubQuery() | SQLExpressionList()) ")"
}

void SQLBetweenClause():
{}
{
    ["NOT"] "BETWEEN" SQLSimpleExpression() "AND" SQLSimpleExpression()
}

void SQLLikeClause():
{}
{
    ["NOT"] "LIKE" SQLSimpleExpression() ["ESCAPE" SQLSimpleExpression()]
}

Expression SQLSimpleExpression():
{ Expression exp1, exp2 = null; Operator op; }
{
    exp1 = SQLMultiplicativeExpression() 
	( ("+" { op = Operator.Add; } | "-" { op = Operator.Subtract; } | "||" { op = Operator.Concat; }) 
	exp2 = SQLMultiplicativeExpression() { exp1 = ParserUtil.Binary(exp1, op, exp2); } )*
	{ return exp1; }
}


Expression SQLMultiplicativeExpression():
{ Expression exp, otherExp = null; Operator op; }
{
    exp = SQLExponentExpression() 
	( ("*" { op = Operator.Multiply; } | "/" { op = Operator.Divide; } ) 
	otherExp = SQLExponentExpression() { exp = ParserUtil.Binary(exp, op, otherExp); } )*
	{ return exp; }
}

Expression SQLExponentExpression():
{ Expression exp, otherExp = null; Operator op; }
{
    exp = SQLUnaryExpression() 
	( "**" otherExp = SQLUnaryExpression() { exp = ParserUtil.Binary(exp, Operator.Exponent, otherExp); } )*
	{ return exp; }
}

Expression SQLUnaryExpression():
{ Expression exp = null; bool isNegative = false; }
{
    ["+" | "-" { isNegative = true; } ] 
	exp = SQLPrimaryExpression()
	{ if (isNegative) exp = Expression.Negative(exp); 
	 return exp; }
}


Expression SQLPrimaryExpression():
{ Expression exp = null; Token t; TableSelectExpression selectExpr = null;
  string columnName = null; }
{
  (
    t = <S_NUMBER> { exp = Expression.Constant(ParserUtil.Number(t.image)); }
  | t = <S_CHAR_LITERAL> { exp = Expression.Constant(ParserUtil.Unquote(t.image));}
  | "NULL" { exp = Expression.Constant(null); }
  | exp = SQLCaseExpression()
  | "(" (LOOKAHEAD(3) selectExpr = Select() { exp = Expression.Subquery(selectExpr); } | 
           exp = SQLExpression()) { exp = Expression.Subset(exp); } ")"
  | BindVariable()
  | LOOKAHEAD(2) exp = SQLCastExpression()
  | LOOKAHEAD(IntervalExpression()) IntervalExpression()
  | LOOKAHEAD(OuterJoinExpression()) OuterJoinExpression()
  | LOOKAHEAD({seeAnalyticFunction()}) AnalyticFunction()
  | LOOKAHEAD(FunctionReference() "(") exp = FunctionCall()
  | columnName =  TableColumn() { exp = Expression.Variable(columnName); } // Might be a call to a parameter-less function.
  )
  { return exp; }
}

Expression SQLCaseExpression():
{ Expression exp = null; 
  Expression exp1 = null, exp2 = null, test = null, ifTrue = null, ifFalse = null; }
{
    "CASE" 
	( ifTrue = SQLSimpleExpression()
		( "WHEN" test = SQLSimpleExpression() 
		  "THEN" ifFalse = SQLSimpleExpression() 
		  { exp1 = Expression.Conditional(test, ifTrue, ifFalse); 
			if (exp != null) exp = Expression.Or(exp, exp1);
			else exp = exp1; } 
		)*
      | ( "WHEN" test = SQLExpression() 
	     "THEN" ifTrue = SQLSimpleExpression() 
		 { 
			exp1 = Expression.Conditional(test, ifTrue);
			if (exp != null) exp = Expression.Or(exp, exp1);
			else exp = exp1;
		} )* 
	)
    ["ELSE" ifFalse = SQLSimpleExpression() { /* TODO: */ } ]
    "END"
	{ return exp; }
}

Expression SQLCastExpression():
{ Expression exp; }
{
    "CAST" "(" exp = SQLExpression() "AS" BasicDataTypeDefinition() ")"
	{ return Expression.FunctionCall("cast", ParserUtil.FunctionArgument(exp)); }
}

void IntervalExpression():
{}
{
    ID("INTERVAL") SQLSimpleExpression()
    ( LOOKAHEAD({"DAY".Equals(GetToken(1).image, StringComparison.OrdinalIgnoreCase)})
      ID("DAY") ["(" <S_NUMBER> ")"] "TO" ID("SECOND") ["(" <S_NUMBER> ")"]
    | ID("YEAR") ["(" <S_NUMBER> ")"] "TO" ID("MONTH") ["(" <S_NUMBER> ")"]
    )
}

Expression FunctionCall():
{ Token t; 
  string name;
  string dateTimeField;
  Expression exp = null; 
  List<FunctionArgument> args = new List<FunctionArgument>(); 
  bool isAll = false;
  bool distinct = false;
}
{
    name = FunctionReference() 
	(
        LOOKAHEAD({"TRIM".Equals(lastObjectReference, StringComparison.OrdinalIgnoreCase)}) args = TrimArguments()
      | LOOKAHEAD({"EXTRACT".Equals(lastObjectReference, StringComparison.OrdinalIgnoreCase)}) 
	    "(" dateTimeField = DatetimeField() { args.Add(new FunctionArgument(Expression.Constant(dateTimeField))); }
		 "FROM" exp = SQLSimpleExpression() { args.Add(new FunctionArgument(exp));} ")"
      | [ "(" [["ALL" { isAll = true;} | "DISTINCT" { distinct = true; } | "UNIQUE"] 
	  ( args =FunctionArgumentList() | 
	  "*" { exp = Expression.Constant("*"); args.Add(new FunctionArgument(exp)); } )] ")" ]
    )
    // "all/distinct/unique/*" are permitted only with aggregate functions,
    // but this parser allows their use with any function.
	{ return Expression.FunctionCall(name, args.ToArray());}
}

string FunctionReference():
{
    String name;
}
{
    name = ObjectReference()
    { return name; }
}

List<FunctionArgument> FunctionArgumentList():
{ List<FunctionArgument> args = new List<FunctionArgument>();
  FunctionArgument arg; }
{
    arg = FunctionArgument() { args.Add(arg); } 
	("," arg = FunctionArgument() { args.Add(arg); } )*
	{ return args; }
}

FunctionArgument FunctionArgument():
{ Token t = null; Expression exp; }
{
    [LOOKAHEAD(2) t = <S_IDENTIFIER> "=>"] 
	exp = SQLExpression()
	{ return new FunctionArgument(t != null ? t.image : null, exp); }
}

List<FunctionArgument> TrimArguments():
{ List<FunctionArgument> args = new List<FunctionArgument>();
  Expression exp = null;
  Token t = null; }
{
    "(" ( LOOKAHEAD({Regex.IsMatch(GetToken(1).image, "(?i)LEADING|TRAILING|BOTH")})
            t = <S_IDENTIFIER> { args.Add(new FunctionArgument(Expression.Constant(t.image))); } 
			[ exp = SQLSimpleExpression() { args.Add(new FunctionArgument(exp)); }] 
			"FROM" exp = SQLSimpleExpression() { args.Add(new FunctionArgument(exp)); }
        | exp = SQLSimpleExpression() { args.Add(new FunctionArgument(exp)); } 
		["FROM" exp = SQLSimpleExpression() { args.Add(new FunctionArgument(exp)); } ]
        )
    ")"
	{ return args; }
}

string DatetimeField():
{ Token t; }
{
   t = <S_IDENTIFIER>
   { return t.image; }
}

String ObjectReference():
{
    String s;
    StringBuilder name = new StringBuilder();
}
{
    s=OracleObjectName()       {name.Append(s);}
    [ "." s=OracleObjectName() {name.Append(".").Append(s);}
    [ "." s=OracleObjectName() {name.Append(".").Append(s);}
    ]]
    [ "@" ("!" | s=TNSName()   {name.Append("@").Append(s);} )] // remote reference
    // The @! idiom is undocumented, but accepted by Oracle software.
    {return lastObjectReference = name.ToString();}
}

void OuterJoinExpression():
{}
{
    TableColumn() "(" "+" ")"
}

TableSelectExpression SubQuery():
{}
{
    SelectStatement() 
	{ return null; }
}

/** Expect an <S_IDENTIFIER> with the given value. */
void ID(String id):
{}
{
    <S_IDENTIFIER>
    {
        if (!Regex.IsMatch(id, "(?i)")) {
            throw new ParseException("Encountered " + token.image
                + " at line " + token.beginLine + ", column " + token.beginColumn + "."
                + "\nWas expecting: " + id);
        }
    }
}
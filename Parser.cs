using System;
using System.Collections.Generic;

namespace TextEditor
{
    public class Parser
    {
        private string input;
        private int pos;
        private List<string> trace = new List<string>();

        public string GetCallTrace() => string.Join("\n", trace);

        public bool Analyze(string text)
        {
            input = text.ToLower();
            pos = 0;
            trace.Clear();
            bool result = BeginStmt();
            return result && pos == input.Length;
        }

        private char Current => pos < input.Length ? input[pos] : '\0';
        private void Advance() { if (pos < input.Length) pos++; }
        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(Current)) Advance();
        }

        private bool Match(string token)
        {
            SkipWhitespace();
            int start = pos;
            foreach (char c in token)
            {
                if (Current != c)
                {
                    pos = start;
                    return false;
                }
                Advance();
            }
            return true;
        }

        private bool BeginStmt()
        {
            trace.Add("begin-stmt");
            int start = pos;
            if (Match("begin") && StmtList() && Match("end"))
                return true;
            pos = start;
            trace.RemoveAt(trace.Count - 1);
            return false;
        }

        private bool StmtList()
        {
            trace.Add("stmt-list");
            int start = pos;
            if (!Stmt())
            {
                pos = start;
                trace.RemoveAt(trace.Count - 1);
                return false;
            }
            while (Match(";"))
            {
                if (!Stmt()) return false;
            }
            return true;
        }

        private bool Stmt()
        {
            trace.Add("stmt");
            int start = pos;
            if (IfStmt()) return true;
            pos = start;
            if (WhileStmt()) return true;
            pos = start;
            if (BeginStmt()) return true;
            pos = start;
            if (AssgStmt()) return true;
            trace.RemoveAt(trace.Count - 1);
            return false;
        }

        private bool IfStmt()
        {
            int start = pos;
            if (Match("if"))
            {
                trace.Add("if-stmt");
                if (BoolExpr() && Match("then") && Stmt() && Match("else") && Stmt()) return true;
                trace.RemoveAt(trace.Count - 1);
                pos = start;
            }
            return false;
        }

        private bool WhileStmt()
        {
            int start = pos;
            if (Match("while"))
            {
                trace.Add("while-stmt");
                if (BoolExpr() && Match("do") && Stmt()) return true;
                trace.RemoveAt(trace.Count - 1);
                pos = start;
            }
            return false;
        }

        private bool AssgStmt()
        {
            trace.Add("assg-stmt");
            int start = pos;
            if (Var() && Match(":=") && ArithExpr()) return true;
            trace.RemoveAt(trace.Count - 1);
            pos = start;
            return false;
        }

        private bool BoolExpr()
        {
            trace.Add("bool-expr");
            int start = pos;
            if (ArithExpr() && CompareOp() && ArithExpr()) return true;
            trace.RemoveAt(trace.Count - 1);
            pos = start;
            return false;
        }

        private bool ArithExpr()
        {
            trace.Add("arith-expr");
            int start = pos;
            if (!Primary())
            {
                trace.RemoveAt(trace.Count - 1);
                pos = start;
                return false;
            }

            while (Match("+") || Match("*"))
            {
                trace.Add("arith-expr");
                if (!Primary()) return false;
            }
            return true;
        }

        private bool Primary()
        {
            int start = pos;
            if (Var()) return true;
            pos = start;
            if (Num()) return true;
            pos = start;
            if (Match("(") && ArithExpr() && Match(")")) return true;
            return false;
        }

        private bool CompareOp()
        {
            trace.Add("compare-op");
            return Match("==") || Match("!=") || Match(">=") || Match("<=") || Match(">") || Match("<");
        }

        private bool Var()
        {
            SkipWhitespace();
            int start = pos;
            if (char.IsLetter(Current))
            {
                Advance();
                while (char.IsLetterOrDigit(Current)) Advance();
                trace.Add("VAR");
                return true;
            }
            pos = start;
            return false;
        }

        private bool Num()
        {
            SkipWhitespace();
            int start = pos;
            if (char.IsDigit(Current))
            {
                while (char.IsDigit(Current)) Advance();
                trace.Add("NUM");
                return true;
            }
            pos = start;
            return false;
        }
    }
}

## Лабораторная работа:  Реализация метода рекурсивного спуска для синтаксического анализа.

## 1. Грамматика

**G[begin-stmt]:**
begin-stmt → begin stmt-list end

stmt-list → stmt | stmt ; stmt-list

stmt → if-stmt | while-stmt | begin-stmt | assg-stmt

if-stmt → if bool-expr then stmt else stmt

while-stmt → while bool-expr do stmt

assg-stmt → VAR := arith-expr

bool-expr → arith-expr compare-op arith-expr

arith-expr → VAR | NUM | (arith-expr) | arith-expr + arith-expr | arith-expr * arith-expr
**Терминалы:**
- `VAR` — переменная: буква (латинская) + (буквы или цифры)  
- `NUM` — число: последовательность цифр  
- `compare-op` — один из: `==`, `!=`, `<`, `<=`, `>`, `>=`

---

## 2. Язык

Данный язык представляет собой подмножество императивного языка программирования, включающее:

- операции **присваивания** (`x := y + 1`)
- конструкции **ветвления** (`if ... then ... else ...`)
- **циклы** (`while ... do ...`)
- **блочные конструкции** (`begin ... end`)
- поддерживает **арифметические выражения** и **сравнения**

---

## 3. Классификация грамматики

Грамматика относится к классу **контекстно-свободных (КС)**, так как все правила соответствуют форме:  
`A → α`, где `A` — нетерминал, `α` — комбинация терминалов и нетерминалов.

---

## 4. Схема вызова функций

### 🔧 Схема вызова функций (дерево рекурсивного спуска)

```text
Analyze
└── BeginStmt
    ├── Match("begin")
    ├── StmtList
    │   ├── Stmt
    │   │   ├── IfStmt
    │   │   │   ├── Match("if")
    │   │   │   ├── BoolExpr
    │   │   │   │   ├── ArithExpr
    │   │   │   │   ├── CompareOp
    │   │   │   │   └── ArithExpr
    │   │   │   ├── Match("then")
    │   │   │   ├── Stmt
    │   │   │   ├── Match("else")
    │   │   │   └── Stmt
    │   │   ├── WhileStmt
    │   │   │   ├── Match("while")
    │   │   │   ├── BoolExpr
    │   │   │   ├── Match("do")
    │   │   │   └── Stmt
    │   │   ├── BeginStmt
    │   │   └── AssgStmt
    │   │       ├── Var
    │   │       ├── Match(":=")
    │   │       └── ArithExpr
    │   └── [';' + Stmt]*
    └── Match("end")

```

## 5. Тестовые примеры

### Корректные примеры:
```pascal
![image](https://github.com/user-attachments/assets/36dee01e-547a-4e6f-91a9-66d070b30001)

![image](https://github.com/user-attachments/assets/f94e9c94-78a0-4751-b401-ebd31e122467)

![image](https://github.com/user-attachments/assets/c6605444-5059-430c-98cd-9374707a72a1)



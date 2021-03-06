﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SymbolicMath.ExpressionHelper;

namespace SymbolicMath
{
    /// <summary>
    /// An operator has two arguments (Expressions) and combines them in some way.
    /// </summary>
    /// <remarks>
    /// When extending this class, remember to pass in the constant value to the constructor <see cref="Function(Expression, double)"/>.
    /// Also, implement the <see cref="Expression.Derivative(string)"/> and <see cref="Expression.Evaluate(Dictionary{string, double})"/> methods.
    /// </remarks>
    public abstract class Operator : Expression
    {
        /// <summary>
        /// The left operand.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// The right operand.
        /// </summary>
        public Expression Right { get; }

        public override bool IsConstant { get { return Left.IsConstant && Right.IsConstant; } }

        public override int Height { get { return Math.Max(Left.Height, Right.Height) + 1; } }

        public override int Size { get { return Left.Size + Right.Size + 1; } }

        public override int Complexity { get { return Left.Complexity + Right.Complexity + 1; } }

        /// <summary>
        /// Indicates if (this(a,b).Equals(this(b, a)) for all a and b.
        /// </summary>
        public abstract bool Commutative { get; }

        /// <summary>
        /// Indicates if (this(this(a,b), c)).Equals(this(a, this(b, c))) for all a, b, and c.
        /// </summary>
        public abstract bool Associative { get; }

        private readonly double m_value;

        public override double Value
        {
            get
            {
                if (!IsConstant)
                {
                    throw new InvalidOperationException("This Function is not constant");
                }
                else
                {
                    return m_value;
                }
            }
        }

        public Operator(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        protected Operator(Expression left, Expression right, double value) : this(left, right)
        {
            m_value = value;
        }

        public abstract Expression With(Expression left, Expression right);

        public override Expression With(IReadOnlyDictionary<Variable, Expression> values)
        {
            return this.With(Left.With(values), Right.With(values));
        }

        public override bool Equals(object obj)
        {
            Operator that = obj as Operator;
            if (that != null)
            {
                bool equal = that.Left.Equals(this.Left) &&
                             that.Right.Equals(this.Right);
                return equal;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Left.GetHashCode() ^ Right.GetHashCode() ^ base.GetHashCode();
        }
    }
    
    public class Power : Operator
    {
        public override bool Commutative { get { return false; } }

        public override bool Associative { get { return false; } }

        internal Power(Expression left, Expression right) : base(left, right, (left.IsConstant && right.IsConstant) ? Math.Pow(left.Value, right.Value) : 0) { }

        internal override Expression DerivativeInternal(Variable variable)
        {
            if (!(Right is Constant) && !(Left is Constant))
            {
                Expression u = Left;
                Expression v = Right;
                Expression du = u.DerivativeInternal(variable);
                Expression dv = v.DerivativeInternal(variable);

                return (new Power(u, v - 1)) * (v * du + u * ln(u) * dv);
            }
            else if ((Right is Constant) && !(Left is Constant))
            {
                Expression u = Left;
                Constant n = Right as Constant;
                Expression du = u.DerivativeInternal(variable);

                return n * (new Power(u, n - 1)) * du;
            }
            else if (!(Right is Constant) && (Left is Constant))
            {
                Constant n = Left as Constant;
                Expression u = Right;
                Expression du = u.DerivativeInternal(variable);

                return n.Log() * (new Power(n, u)) * du;
            }
            else
            //((Right is Constant) && (Left is Constant))
            {
                return 0;
            }
        }

        public override double Evaluate(IReadOnlyDictionary<Variable, double> context)
        {
            return Math.Pow(Left.Evaluate(context), Right.Evaluate(context));
        }

        public override Expression With(Expression left, Expression right)
        {
            return left.Pow(right);
        }

        public override Expression Inv()
        {
            return new Power(Left, -Right);
        }

        public override string ToString()
        {
            return $"({Left.ToString()} ^ {Right.ToString()})";
        }
    }
}

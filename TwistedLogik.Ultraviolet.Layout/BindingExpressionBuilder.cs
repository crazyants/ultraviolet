﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TwistedLogik.Ultraviolet.Layout
{
    /// <summary>
    /// Represents an object which builds a binding expression.
    /// </summary>
    internal class BindingExpressionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingExpressionBuilder"/> class.
        /// </summary>
        /// <param name="viewModelType">The type of view model to which the value is being bound.</param>
        protected BindingExpressionBuilder(Type viewModelType)
        {
            this.viewModelType = viewModelType;
        }

        /// <summary>
        /// Adds a safe reference to the specified component. If accessing the component would result
        /// in a <see cref="NullReferenceException"/>, the getter will return a default value.
        /// </summary>
        /// <param name="current">The current expression in the chain.</param>
        /// <param name="component">The component to which to add a reference.</param>
        /// <param name="conversion">The type to which to convert the reference, if any.</param>
        /// <returns>The variable expression that contains the safe reference.</returns>
        protected Expression AddSafeReference(Expression current, String component, Type conversion = null)
        {
            var reference = (Expression)Expression.PropertyOrField(current, component);
            if (conversion != null)
            {
                reference = Expression.Convert(reference, conversion);
            }

            var variable  = Expression.Variable(reference.Type, "var" + variables.Count);
            variables.Add(variable);

            var assignment = Expression.Assign(variable, reference);
            expressions.Add(assignment);

            if (reference.Type.IsClass)
            {
                AddNullCheck(variable);
            }

            return variable;
        }

        /// <summary>
        /// Adds a null check on the specified variable. If the variable is null, the lambda
        /// will immediately return a default value.
        /// </summary>
        /// <param name="variable">The variable to check for nullity.</param>
        protected void AddNullCheck(ParameterExpression variable)
        {
            var nullCondition = Expression.Equal(variable, Expression.Constant(null));
            var nullCheck = Expression.IfThen(nullCondition, defaultReturnExpression);

            expressions.Add(nullCheck);
        }

        /// <summary>
        /// Creates the lambda's return target label with void type.
        /// </summary>
        protected void CreateReturnTarget()
        {
            exitTarget       = Expression.Label("exit");
            exitLabel        = Expression.Label(exitTarget);
            defaultReturnExpression = Expression.Return(exitTarget);
        }

        /// <summary>
        /// Creates the lambda's return target label with the specified type.
        /// </summary>
        /// <typeparam name="T">The target's type.</typeparam>
        protected void CreateReturnTarget<T>()
        {
            exitTarget       = Expression.Label(typeof(T), "exit");
            exitLabel        = Expression.Label(exitTarget, Expression.Default(typeof(T)));
            defaultReturnExpression = Expression.Return(exitTarget, Expression.Default(typeof(T)));
        }

        /// <summary>
        /// Adds a return label with void type to the lambda.
        /// </summary>
        protected void AddReturnLabel()
        {
            expressions.Add(exitLabel);
        }

        /// <summary>
        /// Adds a return label with the specified type to the lambda.
        /// </summary>
        /// <typeparam name="T">The label's type.</typeparam>
        protected void AddReturnLabel<T>()
        {
            expressions.Add(exitLabel);
        }

        /// <summary>
        /// Adds a return expression with no value.
        /// </summary>
        protected void AddReturn()
        {
            expressions.Add(Expression.Return(exitTarget));
        }

        /// <summary>
        /// Adds a return expression with the specified value.
        /// </summary>
        /// <param name="value">The expression that represents the return value.</param>
        protected void AddReturn(Expression value)
        {
            expressions.Add(Expression.Return(exitTarget, value));
        }

        // State values.
        protected readonly Type viewModelType;
        protected readonly List<Expression> expressions         = new List<Expression>();
        protected readonly List<ParameterExpression> variables  = new List<ParameterExpression>();
        protected readonly List<ParameterExpression> parameters = new List<ParameterExpression>();
        private LabelTarget exitTarget;
        private LabelExpression exitLabel;
        private Expression defaultReturnExpression;
    }
}

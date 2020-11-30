using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Asv.Avialab.Core
{
    internal static class Strings
    {
        internal static string Agregate(string delimiter, params object[] objects)
        {
            if (objects == null) return "";
            objects = objects.Where(x => x != null).ToArray();
            objects = objects.Where(x => !(x is string) || !string.IsNullOrWhiteSpace(x as string)).ToArray();
            if (objects.Length == 0) return "";

            return objects.Select(x => x.ToString()).Aggregate((c, v) => string.Format("{0}{1}{2}", c, delimiter, v));
        }
    }

    public static class ExpressionExtensions
    {
        internal static string GetPropertyFullName(this Expression propertyExpression)
        {
            if (propertyExpression is MemberExpression)
                return GetPropertyName(propertyExpression as MemberExpression);
            else if (propertyExpression is UnaryExpression)
                return GetPropertyName((propertyExpression as UnaryExpression).Operand as MemberExpression);
            else if (propertyExpression is LambdaExpression)
            {
                return GetPropertyFullName((propertyExpression as LambdaExpression).Body);
            }
            else
                throw new ApplicationException(string.Format("Expression: {0} is not MemberExpression",
                    propertyExpression));
        }

        static string GetPropertyName(MemberExpression me)
        {
            string propertyName = me.Member.Name;

            if (me.Expression.NodeType != ExpressionType.Parameter
                && me.Expression.NodeType != ExpressionType.TypeAs
                && me.Expression.NodeType != ExpressionType.Constant)
            {
                propertyName = GetPropertyName(me.Expression as MemberExpression) + "." + propertyName;
            }

            return propertyName;
        }
    }

    public class DeferredExecutor
    {
        public int ExecutionTimeoutMilliseconds { get; set; }
        readonly System.Action _mExecutionAction = null;
        bool _mStarted = false;
        readonly object _mLocker = new object();
        DateTime? _mTimeToExecute = null;
        public string Name { get; set; }

        public int ExecuteCallsCount { get; private set; }

        public DeferredExecutor(System.Action executionAction, int executionTimeoutMilliseconds = 500)
        {
            Name = "(unnamed)";

            if (executionAction == null)
            {
                throw new ArgumentNullException(nameof(executionAction));
            }

            ExecutionTimeoutMilliseconds = executionTimeoutMilliseconds;
            _mExecutionAction = executionAction;
        }

        bool _mNeedRestart = false;

        public void Execute(int? executionTimeoutMilliseconds = null)
        {
            lock (_mLocker)
            {
                ExecuteCallsCount++;

                if (_mTimeToExecute == null) _mTimeToExecute = DateTime.Now;
                if (executionTimeoutMilliseconds == null) executionTimeoutMilliseconds = ExecutionTimeoutMilliseconds;
                _mTimeToExecute = DateTime.Now.AddMilliseconds(executionTimeoutMilliseconds.Value);
                _mNeedRestart = true;

                if (!_mStarted)
                {
                    _mStarted = true;

                    ThreadPool.QueueUserWorkItem(new WaitCallback((object o) =>
                    {
                        try
                        {
                            while (DateTime.Now <= _mTimeToExecute.Value)
                            {
                                Thread.Sleep(1);
                            }

                            if (_mExecutionAction == null)
                                throw new ApplicationException(
                                    string.Format("m_ExecutionAction == null in deferred executor {0}", Name));

                            _mNeedRestart = false;
                            try
                            {
                                _mExecutionAction();
                            }
                            catch (Exception ex)
                            {
                            }
                            finally
                            {
                                _mStarted = false;
                                _mTimeToExecute = null;
                                ExecuteCallsCount = 0;
                            }

                            if (_mNeedRestart) Execute();
                        }
                        catch (Exception ex)
                        {
                        }
                    }));
                }
            }
        }
    }

    public class Deferred
    {
        private Deferred()
        {
        }

        private static readonly Dictionary<System.Action, DeferredExecutor> MActionToExecutor =
            new Dictionary<System.Action, DeferredExecutor>();

        public static void Execute(System.Action action, int executionTimeoutMilliseconds = 500)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            DeferredExecutor executor = null;

            lock (MActionToExecutor)
            {
                if (!MActionToExecutor.ContainsKey(action))
                {
                    MActionToExecutor.Add(action, new DeferredExecutor(action, executionTimeoutMilliseconds));
                }

                executor = MActionToExecutor[action];
            }

            executor.ExecutionTimeoutMilliseconds = executionTimeoutMilliseconds;
            executor.Execute();
        }
    }
}

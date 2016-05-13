using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Krodzone.Rules
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public delegate bool ConditionalValueEvaluationProvider(IConditionalValue value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ConditionMetEventHandler(object sender, EventArgs e);

    #region Enumerations
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum OperationApprovalArgs
    {
        /// <summary>
        /// 
        /// </summary>
        Open = 0,
        /// <summary>
        /// 
        /// </summary>
        InReview = 1,
        /// <summary>
        /// 
        /// </summary>
        Approved = 2,
        /// <summary>
        /// 
        /// </summary>
        Rejected = 3
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// 
    /// </summary>
    public interface IConditionalValue
    {

        #region Events
        /// <summary>
        /// 
        /// </summary>
        event EventHandler ValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        Guid ObjectID { get; }
        /// <summary>
        /// 
        /// </summary>
        bool ItemComplete { get; }
        /// <summary>
        /// 
        /// </summary>
        EvaluationTypeArgs EvaluationType { get; }
        /// <summary>
        /// 
        /// </summary>
        ConditionalValueEvaluationProvider CustomEvaluator { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Equals(IConditionalValue obj);

        /// <summary>
        /// 
        /// </summary>
        void Evaluate();
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConditionalValue<T> : IConditionalValue
    {

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        T TargetValue { get; }
        /// <summary>
        /// 
        /// </summary>
        T MinValue { get; }
        /// <summary>
        /// 
        /// </summary>
        T MaxValue { get; }
        /// <summary>
        /// 
        /// </summary>
        T CurrentValue { get; set; }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IApprovalConditionalValue<T> : IConditionalValue<T>
    {

        #region Events
        /// <summary>
        /// 
        /// </summary>
        event EventHandler OperationApproved;
        /// <summary>
        /// 
        /// </summary>
        event EventHandler OperationRejected;
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICondition
    {

        #region Events
        /// <summary>
        /// 
        /// </summary>
        event ConditionMetEventHandler ConditionMet;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        int Length { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IConditionalValue this[int index] { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int ItemsComplete { get; }
        /// <summary>
        /// 
        /// </summary>
        float PercentComplete { get; }
        /// <summary>
        /// 
        /// </summary>
        ICommonObject ConditionContext { get; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditionalValue"></param>
        void AddConditionalValue(IConditionalValue conditionalValue);
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IConditionCollection
    {

        #region Events
        /// <summary>
        /// 
        /// </summary>
        event ConditionMetEventHandler ConditionMet;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        int Length { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ICondition this[int index] { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int ItemsComplete { get; }
        /// <summary>
        /// 
        /// </summary>
        float PercentComplete { get; }
        /// <summary>
        /// 
        /// </summary>
        ICommonObject ConditionContext { get; }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        void AddCondition(ICondition condition);
        #endregion

    }
    #endregion

    #region Class Objects
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ConditionalValueBase<T> : IConditionalValue<T>
    {

        #region Local Variables
        /// <summary>
        /// 
        /// </summary>
        protected readonly Guid _ObjectID;
        /// <summary>
        /// 
        /// </summary>
        protected T _TargetValue;
        /// <summary>
        /// 
        /// </summary>
        protected bool _ItemComplete;
        /// <summary>
        /// 
        /// </summary>
        protected T _CurrentValue;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ConditionalValueBase()
        {
            _ObjectID = Guid.NewGuid();
            _ItemComplete = false;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public abstract event EventHandler ValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public Guid ObjectID => _ObjectID;

        /// <summary>
        /// 
        /// </summary>
        public T TargetValue => _TargetValue;

        /// <summary>
        /// 
        /// </summary>
        public abstract T MinValue { get; }

        /// <summary>
        /// 
        /// </summary>
        public abstract T MaxValue { get; }

        /// <summary>
        /// 
        /// </summary>
        public abstract EvaluationTypeArgs EvaluationType { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual T CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value.Equals(_CurrentValue)) return;
                _CurrentValue = value;
                OnValueChanged(new EventArgs());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ItemComplete => _ItemComplete;

        /// <summary>
        /// 
        /// </summary>
        public ConditionalValueEvaluationProvider CustomEvaluator { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool Equals(IConditionalValue obj)
        {

            if (obj != null)
            {
                return ObjectID.Equals(obj.ObjectID);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Evaluate()
        {
            OnValueChanged(new EventArgs());
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected abstract void OnValueChanged(EventArgs e);
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class OperationApprovalConditionalValue : ConditionalValueBase<OperationApprovalArgs>, IApprovalConditionalValue<OperationApprovalArgs>
    {

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public OperationApprovalConditionalValue()
        {
            _TargetValue = (OperationApprovalArgs.Approved | OperationApprovalArgs.Rejected);
            _CurrentValue = OperationApprovalArgs.Open;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentValue"></param>
        public OperationApprovalConditionalValue(OperationApprovalArgs currentValue)
        {
            _TargetValue = (OperationApprovalArgs.Approved | OperationApprovalArgs.Rejected);
            _CurrentValue = currentValue;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public override event EventHandler ValueChanged;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler OperationApproved;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler OperationRejected;
        #endregion

        #region Properties
        public override OperationApprovalArgs MinValue => OperationApprovalArgs.Rejected;
        public override OperationApprovalArgs MaxValue => OperationApprovalArgs.Approved;
        public override EvaluationTypeArgs EvaluationType => EvaluationTypeArgs.Custom;

        /// <summary>
        /// 
        /// </summary>
        public override OperationApprovalArgs CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    OnValueChanged(new EventArgs());
                }
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler == null) return;
            _ItemComplete = ((_CurrentValue & (_TargetValue)) != 0);
            handler(this, e);

            if ((_ItemComplete) && (_CurrentValue == OperationApprovalArgs.Approved))
            {
                OnOperationApproved(e);
            }

            if ((_ItemComplete) && (_CurrentValue == OperationApprovalArgs.Rejected))
            {
                OnOperationRejected(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOperationApproved(EventArgs e)
        {
            OperationApproved?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOperationRejected(EventArgs e)
        {
            OperationRejected?.Invoke(this, e);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class MaskedStringConditionalValue : ConditionalValueBase<string>
    {

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringPattern"></param>
        public MaskedStringConditionalValue(string stringPattern)
        {
            _TargetValue = stringPattern;
            _CurrentValue = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringPattern"></param>
        /// <param name="currentValue"></param>
        public MaskedStringConditionalValue(string stringPattern, string currentValue)
        {
            _TargetValue = stringPattern;
            _CurrentValue = currentValue;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public override event EventHandler ValueChanged;
        #endregion

        #region Properties
        public override string MinValue => string.Empty;
        public override string MaxValue => string.Empty;
        public override EvaluationTypeArgs EvaluationType => EvaluationTypeArgs.Custom;

        /// <summary>
        /// 
        /// </summary>
        public override string CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    OnValueChanged(new EventArgs());
                }
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler == null) return;
            _ItemComplete = (Regex.IsMatch(_CurrentValue, TargetValue));
            handler(this, e);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class StringConditionalValue : ConditionalValueBase<string>
    {

        #region Local Variables
        protected readonly EvaluationTypeArgs _EvaluationType;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="evaluationType"></param>
        public StringConditionalValue(string targetValue, EvaluationTypeArgs evaluationType)
        {
            _TargetValue = targetValue;
            _EvaluationType = evaluationType;
            _CurrentValue = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="evaluationType"></param>
        /// <param name="currentValue"></param>
        public StringConditionalValue(string targetValue, EvaluationTypeArgs evaluationType, string currentValue)
        {
            _TargetValue = targetValue;
            _EvaluationType = evaluationType;
            _CurrentValue = currentValue;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public override event EventHandler ValueChanged;
        #endregion

        #region Properties
        public override string MinValue => string.Empty;
        public override string MaxValue => string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public override EvaluationTypeArgs EvaluationType => _EvaluationType;

        /// <summary>
        /// 
        /// </summary>
        public override string CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    OnValueChanged(new EventArgs());
                }
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler == null) return;
            switch (EvaluationType)
            {
                case EvaluationTypeArgs.Exact:
                    _ItemComplete = (TargetValue.Equals(_CurrentValue, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case EvaluationTypeArgs.Negation:
                    _ItemComplete = (TargetValue.Equals(_CurrentValue, StringComparison.InvariantCultureIgnoreCase));
                    break;
                case EvaluationTypeArgs.Custom:
                    _ItemComplete = (CustomEvaluator != null && CustomEvaluator(this));
                    break;
                default:
                    _ItemComplete = false;
                    break;
            }

            handler(this, e);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class IntegerConditionalValue : ConditionalValueBase<int>
    {

        #region Local Variables
        protected readonly int _MinValue = -1;
        protected readonly int _MaxValue = -1;
        protected readonly EvaluationTypeArgs _EvaluationType;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="evaluationType"></param>
        public IntegerConditionalValue(int targetValue, EvaluationTypeArgs evaluationType)
        {
            _TargetValue = targetValue;
            _EvaluationType = (evaluationType != EvaluationTypeArgs.Range ? evaluationType : EvaluationTypeArgs.Exact);
            _CurrentValue = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public IntegerConditionalValue(int minValue, int maxValue)
        {
            _TargetValue = -1;
            _MinValue = minValue;
            _MaxValue = maxValue;
            _EvaluationType = EvaluationTypeArgs.Range;
            _CurrentValue = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="evaluationType"></param>
        /// <param name="currentValue"></param>
        public IntegerConditionalValue(int targetValue, EvaluationTypeArgs evaluationType, int currentValue)
        {
            _TargetValue = targetValue;
            _EvaluationType = (evaluationType != EvaluationTypeArgs.Range ? evaluationType : EvaluationTypeArgs.Exact);
            _CurrentValue = currentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="currentValue"></param>
        public IntegerConditionalValue(int minValue, int maxValue, int currentValue)
        {
            _TargetValue = -1;
            _MinValue = minValue;
            _MaxValue = maxValue;
            _EvaluationType = EvaluationTypeArgs.Range;
            _CurrentValue = currentValue;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public override event EventHandler ValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public override EvaluationTypeArgs EvaluationType => _EvaluationType;

        /// <summary>
        /// 
        /// </summary>
        public override int MinValue => _MinValue;

        /// <summary>
        /// 
        /// </summary>
        public override int MaxValue => _MaxValue;

        /// <summary>
        /// 
        /// </summary>
        public override int CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    OnValueChanged(new EventArgs());
                }
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler == null) return;
            switch (EvaluationType)
            {
                case EvaluationTypeArgs.Exact:
                    _ItemComplete = (_TargetValue == _CurrentValue);
                    break;
                case EvaluationTypeArgs.Negation:
                    _ItemComplete = (_TargetValue != _CurrentValue);
                    break;
                case EvaluationTypeArgs.Range:

                    if (MinValue == -1 && MaxValue == -1)
                    {
                        _ItemComplete = true;
                    }

                    if (MinValue == -1 && MaxValue > MinValue)
                    {
                        _ItemComplete = (_CurrentValue <= MaxValue);
                    }

                    if (MinValue != -1 && MaxValue == -1)
                    {
                        _ItemComplete = (_CurrentValue >= MinValue);
                    }

                    if (MinValue != -1 && MaxValue != -1)
                    {
                        _ItemComplete = (_CurrentValue >= MinValue && _CurrentValue <= MaxValue);
                    }

                    break;
                case EvaluationTypeArgs.Custom:
                    _ItemComplete = (CustomEvaluator != null && CustomEvaluator(this));
                    break;
                default:
                    _ItemComplete = false;
                    break;
            }

            handler(this, e);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class FloatConditionalValue : ConditionalValueBase<float>
    {

        #region Local Variables
        protected readonly float _MinValue = -1;
        protected readonly float _MaxValue = -1;
        protected readonly EvaluationTypeArgs _EvaluationType;
        #endregion

        #region Constructor
        public FloatConditionalValue(float targetValue, EvaluationTypeArgs evaluationType)
        {
            _TargetValue = targetValue;
            _EvaluationType = (evaluationType != EvaluationTypeArgs.Range ? evaluationType : EvaluationTypeArgs.Exact);
            _CurrentValue = 0;
        }

        public FloatConditionalValue(float minValue, float maxValue)
        {
            _TargetValue = -1;
            _MinValue = minValue;
            _MaxValue = maxValue;
            _EvaluationType = EvaluationTypeArgs.Range;
            _CurrentValue = 0;
        }

        public FloatConditionalValue(float targetValue, EvaluationTypeArgs evaluationType, float currentValue)
        {
            _TargetValue = targetValue;
            _EvaluationType = (evaluationType != EvaluationTypeArgs.Range ? evaluationType : EvaluationTypeArgs.Exact);
            _CurrentValue = currentValue;
        }

        public FloatConditionalValue(float minValue, float maxValue, float currentValue)
        {
            _TargetValue = -1;
            _MinValue = minValue;
            _MaxValue = maxValue;
            _EvaluationType = EvaluationTypeArgs.Range;
            _CurrentValue = currentValue;
        }
        #endregion

        #region Event Handlers
        public override event EventHandler ValueChanged;
        #endregion

        #region Properties
        public override EvaluationTypeArgs EvaluationType => _EvaluationType;

        public override float MinValue => _MinValue;

        public override float MaxValue => _MaxValue;

        public override float CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    OnValueChanged(new EventArgs());
                }
            }
        }
        #endregion

        #region Protected Methods
        protected override void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler == null) return;
            switch (EvaluationType)
            {
                case EvaluationTypeArgs.Exact:
                    _ItemComplete = (_TargetValue == _CurrentValue);
                    break;
                case EvaluationTypeArgs.Negation:
                    _ItemComplete = (_TargetValue != _CurrentValue);
                    break;
                case EvaluationTypeArgs.Range:

                    if (MinValue == -1 && MaxValue == -1)
                    {
                        _ItemComplete = true;
                    }

                    if (MinValue == -1 && MaxValue > MinValue)
                    {
                        _ItemComplete = (_CurrentValue <= MaxValue);
                    }

                    if (MinValue != -1 && MaxValue == -1)
                    {
                        _ItemComplete = (_CurrentValue >= MinValue);
                    }

                    if (MinValue != -1 && MaxValue != -1)
                    {
                        _ItemComplete = (_CurrentValue >= MinValue && _CurrentValue <= MaxValue);
                    }

                    break;
                case EvaluationTypeArgs.Custom:
                    _ItemComplete = (CustomEvaluator != null && CustomEvaluator(this));
                    break;
                default:
                    _ItemComplete = false;
                    break;
            }

            handler(this, e);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class DateConditionalValue : ConditionalValueBase<DateTime>
    {

        #region Local Variables
        protected readonly DateTime _MinValue = new DateTime(1900, 1, 1);
        protected readonly DateTime _MaxValue = new DateTime(1900, 1, 1);
        protected readonly EvaluationTypeArgs _EvaluationType;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="evaluationType"></param>
        public DateConditionalValue(DateTime targetValue, EvaluationTypeArgs evaluationType)
        {
            _TargetValue = targetValue;
            _EvaluationType = (evaluationType != EvaluationTypeArgs.Range ? evaluationType : EvaluationTypeArgs.Exact);
            _CurrentValue = new DateTime(1900, 1, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public DateConditionalValue(DateTime minValue, DateTime maxValue)
        {
            _TargetValue = new DateTime(1900, 1, 1);
            _MinValue = minValue;
            _MaxValue = maxValue;
            _EvaluationType = EvaluationTypeArgs.Range;
            _CurrentValue = new DateTime(1900, 1, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"></param>
        /// <param name="evaluationType"></param>
        /// <param name="currentValue"></param>
        public DateConditionalValue(DateTime targetValue, EvaluationTypeArgs evaluationType, DateTime currentValue)
        {
            _TargetValue = targetValue;
            _EvaluationType = (evaluationType != EvaluationTypeArgs.Range ? evaluationType : EvaluationTypeArgs.Exact);
            _CurrentValue = currentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="currentValue"></param>
        public DateConditionalValue(DateTime minValue, DateTime maxValue, DateTime currentValue)
        {
            _TargetValue = new DateTime(1900, 1, 1);
            _MinValue = minValue;
            _MaxValue = maxValue;
            _EvaluationType = EvaluationTypeArgs.Range;
            _CurrentValue = currentValue;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public override event EventHandler ValueChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public override EvaluationTypeArgs EvaluationType => _EvaluationType;

        /// <summary>
        /// 
        /// </summary>
        public override DateTime MinValue => _MinValue;

        /// <summary>
        /// 
        /// </summary>
        public override DateTime MaxValue => _MaxValue;

        /// <summary>
        /// 
        /// </summary>
        public override DateTime CurrentValue
        {
            get { return _CurrentValue; }
            set
            {
                if (value != _CurrentValue)
                {
                    _CurrentValue = value;
                    OnValueChanged(new EventArgs());
                }
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler == null) return;
            switch (EvaluationType)
            {
                case EvaluationTypeArgs.Exact:
                    _ItemComplete = (_TargetValue == _CurrentValue);
                    break;
                case EvaluationTypeArgs.Negation:
                    _ItemComplete = (_TargetValue != _CurrentValue);
                    break;
                case EvaluationTypeArgs.Range:

                    if (MinValue.Year == 1900 && MaxValue.Year == 1900)
                    {
                        _ItemComplete = true;
                    }

                    if (MinValue.Year == 1900 && MaxValue.Year > 1900)
                    {
                        _ItemComplete = (_CurrentValue <= MaxValue);
                    }

                    if (MinValue.Year > 1900 && MaxValue.Year == 1900)
                    {
                        _ItemComplete = (_CurrentValue >= MinValue);
                    }

                    if (MinValue.Year > 1900 && MaxValue.Year > 1900)
                    {
                        _ItemComplete = (_CurrentValue >= MinValue && _CurrentValue <= MaxValue);
                    }

                    break;
                case EvaluationTypeArgs.Custom:
                    _ItemComplete = (CustomEvaluator != null && CustomEvaluator(this));
                    break;
                default:
                    _ItemComplete = false;
                    break;
            }

            handler(this, e);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class Condition : ICondition
    {

        #region Local Variables
        protected readonly List<IConditionalValue> _Items;
        protected ICommonObject _ConditionContext;
        #endregion

        #region Constructors
        public Condition(ICommonObject conditionContext)
        {
            _Items = new List<IConditionalValue>();
            _ConditionContext = conditionContext;
        }
        #endregion

        #region Event Handlers
        public event ConditionMetEventHandler ConditionMet;
        #endregion

        #region Properties
        public int Length => _Items.Count;

        public IConditionalValue this[int index]
        {
            get { return _Items[index]; }
            set
            {
                if (index >= 0 && index <= _Items.Count - 1)
                {
                    _Items[index] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException("The value for the index exceeds the total items in the collection.");
                }
            }
        }

        public int ItemsComplete => GetTotalItemsComplete();

        public float PercentComplete
        {
            get
            {
                if (_Items.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return ((float)GetTotalItemsComplete() / _Items.Count);
                }
            }
        }

        public ICommonObject ConditionContext => _ConditionContext;

        #endregion

        #region Public Methods
        public virtual void AddConditionalValue(IConditionalValue conditionalValue)
        {
            if (!_Items.Contains(conditionalValue))
            {
                bool itemFound = false;

                foreach (IConditionalValue item in _Items)
                {
                    itemFound = conditionalValue.Equals(item);
                    if (itemFound) { break; }
                }

                if (!itemFound)
                {
                    conditionalValue.ValueChanged += OnValueChanged;
                    _Items.Add(conditionalValue);
                    conditionalValue.Evaluate();
                }

            }
        }
        #endregion

        #region Protected Methods
        protected virtual void OnConditionMet(EventArgs e)
        {
            ConditionMet?.Invoke(this, e);
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (PercentComplete >= 1) { OnConditionMet(new EventArgs()); }
        }
        #endregion

        #region Private Methods
        private int GetTotalItemsComplete()
        {
            int total = 0;

            foreach (IConditionalValue item in _Items)
            {
                if (item.ItemComplete) { total++; }
            }

            return total;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class ApprovalCondition : Condition
    {

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditionalValue"></param>
        /// <param name="conditionContext"></param>
        public ApprovalCondition(IApprovalConditionalValue<OperationApprovalArgs> conditionalValue, ICommonObject conditionContext)
            : base(conditionContext)
        {
            conditionalValue.OperationApproved += OnOperationApproved;
            conditionalValue.OperationRejected += OnOperationRejected;
            _Items.Add(conditionalValue);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler OperationApproved;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler OperationRejected;
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditionalValue"></param>
        public override void AddConditionalValue(IConditionalValue conditionalValue)
        {
            throw new NotImplementedException("This class only allows one condition, which is assigned at creation.");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="send"></param>
        /// <param name="e"></param>
        private void OnOperationApproved(object send, EventArgs e)
        {
            OperationApproved?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOperationRejected(object sender, EventArgs e)
        {
            OperationRejected?.Invoke(this, e);
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class ConditionCollection : IConditionCollection
    {

        #region Local Variables
        /// <summary>
        /// 
        /// </summary>
        protected readonly List<ICondition> _Items;
        /// <summary>
        /// 
        /// </summary>
        protected ICommonObject _ConditionContext;
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditionContext"></param>
        public ConditionCollection(ICommonObject conditionContext)
        {
            _Items = new List<ICondition>();
            _ConditionContext = conditionContext;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        public event ConditionMetEventHandler ConditionMet;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public int Length => _Items.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ICondition this[int index]
        {
            get { return _Items[index]; }
            set
            {
                if (index >= 0 && index <= _Items.Count - 1)
                {
                    _Items[index] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException("The value for the index exceeds the total items in the collection.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ItemsComplete => GetTotalCondtionsMet();

        /// <summary>
        /// 
        /// </summary>
        public float PercentComplete
        {
            get
            {
                if (_Items.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return ((float)GetTotalCondtionsMet() / _Items.Count);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommonObject ConditionContext => _ConditionContext;

        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        public virtual void AddCondition(ICondition condition)
        {
            if (!_Items.Contains(condition))
            {
                bool itemFound = false;

                foreach (ICondition item in _Items)
                {
                    itemFound = condition.Equals(item);
                    if (itemFound) { break; }
                }

                if (!itemFound)
                {
                    condition.ConditionMet += OnConditionItemConditionMet;
                    _Items.Add(condition);
                }

            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConditionMet(EventArgs e)
        {
            ConditionMet?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnConditionItemConditionMet(object sender, EventArgs e)
        {
            int conditionsMet = 0;

            foreach (ICondition item in _Items)
            {
                if (item.PercentComplete >= 1.0)
                {
                    conditionsMet += 1;
                }
            }

            if (conditionsMet >= _Items.Count) { OnConditionMet(new EventArgs()); }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int GetTotalCondtionsMet()
        {
            int conditionsMet = 0;

            foreach (ICondition item in _Items)
            {
                if (item.PercentComplete >= 1.0)
                {
                    conditionsMet += 1;
                }
            }

            return conditionsMet;

        }
        #endregion

    }
    #endregion

}

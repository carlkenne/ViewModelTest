using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace ViewMockBase
{
    /// <summary>
    /// Simulates a View and records all NotifyPropertyChanged Events that is fired from the viewModel
    /// </summary>
    public class ViewMock
    {
        /// <summary>
        /// creates a new Viewmock that monitors all properties
        /// </summary>
        public static ViewMock<T> Observe<T>(T viewModel) where T : INotifyPropertyChanged
        {
            var view = new ViewMock<T>(viewModel);
            view.SaveCurrentFullState();
            return view;
        }

        /// <summary>
        /// creates a new Viewmock that monitors all properties
        /// </summary>
        public static ViewMockBuilder<T> ObservePartial<T>(T viewModel) where T : INotifyPropertyChanged
        {
            return new ViewMockBuilder<T>(viewModel);
        }
    }

    /// <summary>
    /// Simulates a View and records all NotifyPropertyChanged Events that is fired from the viewModel
    /// </summary>
    public class ViewMock<TViewModel> where TViewModel : INotifyPropertyChanged
    {
        protected readonly INotifyPropertyChanged _viewModel;
        readonly Dictionary<string, object> _savedProperties = new Dictionary<string, object>();
        private string _lastError = String.Empty;

        internal ViewMock(TViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += viewModel_PropertyChanged;
        }

        internal void SaveCurrentFullState()
        {
            foreach (var property in _viewModel.GetType().GetProperties())
            {
                _savedProperties[property.Name] = property.GetValue(_viewModel, null);
            }
        }
        
        protected void SaveStateFor(PropertyInfo property)
        {
            _savedProperties[property.Name] = property.GetValue(_viewModel, null);
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == string.Empty)
            {
                SaveCurrentFullState();
                return;
            }

            var property = _viewModel.GetType().GetProperty(e.PropertyName);
            if (property == null)
            {
                throw new ArgumentNullException("Property of name " + e.PropertyName + " does not exist");
            }
            var value = property.GetValue(_viewModel, null);
            _savedProperties[e.PropertyName] = value;
        }

        /// <summary>
        /// Returns the property as it is displayed by the view
        /// </summary>
        public object Property(string propertyName)
        {
            object value;
            if(!_savedProperties.TryGetValue(propertyName, out value))
            {
                throw new ArgumentException(string.Format("The property {0} is not being observed in this test. To Observe a specific property use ViewMock.ObservePartial().WithProperty(<propertyName>)", propertyName));
            }
            return value;
        }

        /// <summary>
        /// Returns the value of the property as it is displayed by the view
        /// </summary>
        public TValue Property<TValue>(string propertyName)
        {
            object value;
            if (!_savedProperties.TryGetValue(propertyName, out value))
            {
                throw new ArgumentException(string.Format("The property {0} is not being observed in this test. To Observe a specific property use ViewMock.ObservePartial().WithProperty(<propertyName>)", propertyName));
            }
            if(!(value is  TValue))
            {
                throw new ArgumentException(string.Format("The property {0} is not of specified type: {1}", propertyName, typeof(TValue)));
            }
            return (TValue) value;
        }

        /// <summary>
        /// Returns the actual value of the property from the viewmodel
        /// </summary>
        public object ActualProperty(string propertyname)
        {
            var prop = _viewModel.GetType().GetProperty(propertyname);
            return prop.GetValue(_viewModel, null);
        }

        /// <summary>
        /// Returns the actual value of the property from the viewmodel
        /// </summary>
        public TValue ActualProperty<TValue>(Expression<Func<TViewModel, TValue>> propertyExpression)
        {
            return ExpressionHelper.GetPropertyValue<TValue>(ExpressionHelper.GetProperty(propertyExpression), _viewModel);
        }

        /// <summary>
        /// Returns the value of the property as it is displayed by the view
        /// </summary>
        public TValue Property<TValue>(Expression<Func<TViewModel, TValue>> propertyExpression)
        {
            var name = ExpressionHelper.GetPropertyNameFrom(propertyExpression);
            return (TValue) Property(name);
        }

        /// <summary>
        /// Returns the propertyInfo for expression
        /// </summary>
        public PropertyInfo PropertyInfo<TValue>(Expression<Func<TViewModel, TValue>> property)
        {
            return ExpressionHelper.GetProperty(property);
        }

        public bool IsDisplayedAs<TValue>(Expression<Func<TViewModel,TValue>> property, TValue desiredValue)
            where TValue : class
        {
            var displayValue = Property(property);
            var actualValue = ActualProperty(property);
            if ((displayValue == null) || (actualValue.Equals(desiredValue) && !displayValue.Equals(desiredValue)))
            {
                _lastError = "The viewModel is correct but the view was never notified with NotifyPropertyChanged.";
            }
            return displayValue == desiredValue;
        }

        public string GetLastError()
        {
            var ret = _lastError;
            _lastError = String.Empty;
            return ret;
        }
    }
}
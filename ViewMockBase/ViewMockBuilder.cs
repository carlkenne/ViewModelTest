using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ViewMockBase
{
    /// <summary>
    /// Builds a ViewMock that observes specific properties only
    /// </summary>
    public class ViewMockBuilder<T> : ViewMock<T>
        where T : INotifyPropertyChanged
    {
        internal ViewMockBuilder(T viewModel)
            : base(viewModel)
        { }

        /// <summary>
        /// Specifies a property to watch
        /// </summary>
        public ViewMockBuilder<T> WithProperty(Expression<Func<T, object>> property)
        {
            var propertyInfo = ExpressionHelper.GetProperty(property);
            SaveStateFor(propertyInfo);
            return this;
        }

        /// <summary>
        /// Specifies a property to watch
        /// </summary>
        public ViewMockBuilder<T> WithProperty(string propertyName)
        {
            var propertyInfo = _viewModel.GetType().GetProperty(propertyName);
            SaveStateFor(propertyInfo);
            return this;
        }
    }
}
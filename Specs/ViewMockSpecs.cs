using System;
using System.ComponentModel;
using NUnit.Framework;
using ViewMockBase;

namespace Specs
{
    /// <summary>
    /// This spec shows basic usage
    /// </summary>
    [TestFixture]
    public class when_observing_a_viewModel
    {
        private ViewMock<ViewModel> _view;

        [SetUp]
        public void Context()
        {
            _view = ViewMock.Observe(new ViewModel { Name = "One" });
        }

        [Test]
        public void It_can_retrieve_the_correct_value_by_lambda()
        {
            Assert.That(_view.Property(vm => vm.Name), Is.EqualTo("One"));
        }

        [Test]
        public void It_can_retrieve_the_correct_value_by_string()
        {
            Assert.That(_view.Property("Name"), Is.EqualTo("One"));
        }
    }

    /// <summary>
    /// This spec shows how the viewMock can distinguish between 
    /// what the actual value is and what value that would be shown 
    /// if the ViewModel was bound to a real XAML-view
    /// </summary>
    [TestFixture]
    public class when_the_name_property_has_been_changed_without_notification
    {
        private ViewMock<ViewModel> _view;

        [SetUp]
        public void Context()
        {
            var viewModel = new ViewModel { Name = "Default" };
            _view = ViewMock.Observe(viewModel);

            viewModel.Name = "Not Notified";
        }

        [Test]
        public void It_should_update_the_viewModel_with_the_correct_name()
        {
            Assert.That(_view.ActualProperty(vm => vm.Name), Is.EqualTo("Not Notified"));
        }

        [Test]
        public void It_should_still_show_the_old_name_in_the_view_since_no_notifiction_has_been_sent()
        {
            Assert.That(_view.Property(vm => vm.Name),Is.EqualTo("Default"));
        }

        /// <summary>
        /// Since the view knows what values is shown and not.
        /// Wouldnt it be nice if it could tell us right a way
        /// in the test if we had forget to send a notifypropertychanged?
        /// </summary>
        [Test]
        public void It_should_be_able_to_tell_us_if_the_value_is_incorrect()
        {
            var isDisplayedAs = _view.IsDisplayedAs(wm => wm.Name, "Not Notified");
            Assert.That(isDisplayedAs, Is.False);
        }

        /// <summary>
        /// this should be used as:
        /// Assert.That(_view.IsDisplayedAs(wm => wm.Name, "Notified"), _view.GetLastError());
        /// </summary>
        [Test]
        public void It_should_also_tell_us_why()
        {
            _view.IsDisplayedAs(wm => wm.Name, "Not Notified");
            Assert.That(_view.GetLastError(), Is.EqualTo("The viewModel is correct but the view was never notified with NotifyPropertyChanged."));
        }
    }

    [TestFixture]
    public class when_the_name_property_has_been_changed_with_notification
    {
        [Test]
        public void It_should_display_the_new_correct_name_in_the_view()
        {
            var viewModel = new ViewModel { Name = "Default" };
            var view = ViewMock.Observe(viewModel);

            viewModel.ChangeNameAndNotify("Notified Name");

            Assert.That(view.Property(vm => vm.Name), Is.EqualTo("Notified Name"));
        }
    }

    /// <summary>
    /// one way to notify the view in XAML is to pass an empty string
    /// to NotifyPropertyChanged. This tells the view to refresh all 
    /// Properties
    /// </summary>
    [TestFixture]
    public class when_the_name_property_has_been_changed_and_notified_with_an_empty_string
    {
        [Test]
        public void It_should_display_the_new_updated_name_in_the_view()
        {
            var viewModel = new ViewModel { Name = "Default" };
            var view = ViewMock.Observe(viewModel);

            viewModel.Name = "Notified Name";
            viewModel.NotifyWithEmptyString();

            Assert.That(view.Property(vm => vm.Name), Is.EqualTo("Notified Name"));
        }
    }
    
    /// <summary>
    /// A ViewMock will initially ask the viewmodel of the values 
    /// of all its properties it is observing. This is not always 
    /// desirable, some properties may be initialized later depending 
    /// on the viewModel design.
    /// By using the ObservePartial, specific properties may be specified
    /// to observe instead of observing all (default)
    /// </summary>
    [TestFixture]
    public class when_the_view_is_observing_specific_properties
    {
        private ViewMockBuilder<ViewModel> _view;

        [SetUp]
        public void Context()
        {
            _view = ViewMock.ObservePartial(new ViewModel { Name = "One" })
              .WithProperty(vm => vm.Name);
        }

        [Test]
        public void It_should_throw_an_excpetion_when_asked_for_a_property_that_it_is_not_observing()
        {
            Assert.Throws(typeof(ArgumentException), () => _view.Property(vm => vm.UnobservedProperty));
        }

        [Test]
        public void it_should_now_the_values_of_properties_that_it_is_observing()
        {
            Assert.That(_view.Property(vm => vm.Name), Is.EqualTo("One"));
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }
        public string UnobservedProperty { get; set; }

        public void ChangeNameAndNotify(string value)
        {
            Name = value;
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));
        }

        public void NotifyWithEmptyString()
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}
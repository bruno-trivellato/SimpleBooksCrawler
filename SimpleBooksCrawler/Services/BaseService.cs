using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBooksCrawler.Services
{
    public partial class BaseService
    {
        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event ServicePropertyChangedEventHandler ServicePropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetServiceProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null, bool enforceEventRise = false)
        {
            if (object.Equals(storage, value) && enforceEventRise == false) return false;

            storage = value;


            this.OnServicePropertyChanged(value, propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnServicePropertyChanged(object value, [CallerMemberName] string propertyName = null)
        {
            this.ServicePropertyChanged?.Invoke(this, new ServicePropertyChangedEventArgs(propertyName, value));
        }

        public delegate void ServicePropertyChangedEventHandler(object sender, ServicePropertyChangedEventArgs e);

        public class ServicePropertyChangedEventArgs
        {
            public String ServicePropertyName { get; set; }
            public object ServicePropertyValue { get; set; }


            public ServicePropertyChangedEventArgs(String propertyName, object value)
            {
                this.ServicePropertyName = propertyName;
                this.ServicePropertyValue = value;
            }
        }

    }
}

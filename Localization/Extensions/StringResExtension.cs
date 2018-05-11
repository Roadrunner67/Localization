using Localization.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace Localization.Extensions
{
    [MarkupExtensionReturnType(typeof(string))]
    public class StringResExtension: MarkupExtension
    {
        /// <summary>
        /// Static list of stingextensions (used for later update).
        /// </summary>
        private static readonly List<StringResExtension> Instances;

        /// <summary>
        /// Target object for this instance.
        /// </summary>
        private WeakReference _targetObject;

        /// <summary>
        /// Target object for this instance.
        /// </summary>
        // ?? private List<WeakReference> targetObjects;

        /// <summary>
        /// Target property for this instance.
        /// </summary>
        private object _targetProperty;

        /// <summary>
        /// Method to get a translated value.
        /// </summary>
        /// <param name="lookUpName">Name of string to look up</param>
        /// <returns>The translated result</returns>
        public static string LookUp(string lookUpName)
        {
            return new StringResExtension(lookUpName).Value;
        }

        /// <summary>
        /// Get or set the name of the string.
        /// </summary>
        [ConstructorArgument("name")]
        public string Name
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get string value.
        /// </summary>
        private string Value => Resources.ResourceManager.GetString(Name);

        /// <summary>
        /// Determines if the target object still exist (or has been collected by the garbage collector)?
        /// </summary>
        private bool IsAlive => (_targetObject?.Target != null && (_targetProperty != null));


        /// <summary>
        /// Static constructor.
        /// </summary>
        static StringResExtension()
        {
            // Create list instance.
            Instances = new List<StringResExtension>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">String identifier.</param>
        public StringResExtension(string name)
        {
            // Set name.
            Name = name;
        }


        /// <summary>
        /// Update all registered string extensions.
        /// </summary>
        public static void UpdateStrings()
        {
            // Create temporary removal list.
            List<StringResExtension> instancesToRemove = new List<StringResExtension>();

            // Iterate through all strings extension instances.
            foreach (StringResExtension instance in Instances)
            {
                // Is this instance alive?
                if (instance.IsAlive)
                {
                    // Update target property.
                    instance.UpdateTarget();
                }
                else
                {
                    // Add it to our remove list.
                    instancesToRemove.Add(instance);
                }
            }

            // Iterate through all instances to remove.
            foreach (StringResExtension instance in instancesToRemove)
            {
                // Remove the instance from our list.
                Instances.Remove(instance);
            }
        }


        /// <summary>
        /// Retrieve and return the string (only called once).
        /// If the target object is of type <see cref="DependencyObject"/> a string is returned, otherwise <value>this</value> <see cref="StringResExtension"/> is returned.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns>
        /// If the target object is of type <see cref="DependencyObject"/> a string is returned, otherwise <code>this</code> <see cref="StringResExtension"/> is returned.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Retrieve provider service.
            var service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (service == null)
                return this;

            if (!(service.TargetObject is DependencyObject) && !(service.TargetProperty is MemberInfo))
                return this;

            // Do we have a dependency object?
            if (service.TargetObject is DependencyObject)
            {
                // Save the target object.
                _targetObject = new WeakReference(service.TargetObject);

                // Save the target property.
                _targetProperty = service.TargetProperty;

                // Add it to our list.
                Instances.Add(this);
                System.Diagnostics.Trace.WriteLine(this.Name + " added.");

                return Value;
            }

            return this;
        }


        /// <summary>
        /// Update the target property.
        /// </summary>
        private void UpdateTarget()
        { 
            // Do we have a target object and property?
            if(IsAlive)
            {
                try
                {
                    // Set property on target object
                    var targetProperty = _targetProperty as DependencyProperty;
                    if (targetProperty != null)
                    {
                        var dependencyObject = _targetObject.Target as DependencyObject;                        
                        var dependencyProperty = targetProperty;
                        dependencyObject?.SetValue(dependencyProperty, Value);
                    }
                }
                catch
                {
                    System.Diagnostics.Trace.WriteLine($"StringResExtension failed to update target ({_targetObject.Target},{_targetProperty}).");
                }            
            }
        }
    }
}

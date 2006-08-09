// Stephen Toub
// stoub@microsoft.com

namespace NetMatters
{
    #region Imports

    using System;
    using System.Reflection;
    using System.ComponentModel;

    #endregion
    
    internal sealed class FieldsToPropertiesProxyTypeDescriptor : ICustomTypeDescriptor
	{
		private readonly Type _type;
        private PropertyDescriptorCollection _cachedProperties;
        private FilterCache _filterCache;

		public FieldsToPropertiesProxyTypeDescriptor(Type type)
		{
			if (type == null) 
			    throw new ArgumentNullException("type");
		    
			_type = type;
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor property)
		{
			return _type;
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(_type, true);
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(_type, true);
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(_type, true);
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(_type, true);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(_type, true);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(_type, true);
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(_type, editorBaseType, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(_type, attributes, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(_type, true);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return ((ICustomTypeDescriptor) this).GetProperties(null);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			bool filtering = (attributes != null && attributes.Length > 0);
			PropertyDescriptorCollection properties = _cachedProperties;
			FilterCache cache = _filterCache;

			//
		    // Use a cached version if we can.
		    //
		    
			if (filtering && cache != null && cache.IsValid(attributes)) 
			{
				return cache.FilteredProperties;
			}
			else if (!filtering && properties != null) 
			{
				return properties;
			}

			//
		    // Create the property collection and filter if necessary.
		    //
		    
			properties = new PropertyDescriptorCollection(null);
			
		    foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(_type, attributes))
				properties.Add(property);
		    
			foreach (FieldInfo field in _type.GetFields())
			{
				FieldPropertyDescriptor fieldProperty = new FieldPropertyDescriptor(field);
			    
				if (!filtering || fieldProperty.Attributes.Contains(attributes)) 
				    properties.Add(fieldProperty);
			}

		    //
			// Store the computed properties.
		    //
		    
			if (filtering) 
			{
				cache = new FilterCache();
				cache.Attributes = attributes;
				cache.FilteredProperties = properties;
				_filterCache = cache;
			} 
			else
			{
			    _cachedProperties = properties;
			}

			return properties;
		}

		private class FilterCache
		{
			public Attribute[] Attributes;
			public PropertyDescriptorCollection FilteredProperties;
			public bool IsValid(Attribute[] other)
			{
				if (other == null || Attributes == null) return false;
				if (Attributes.Length != other.Length) return false;
				for (int i = 0; i < other.Length; i++)
				{
					if (!Attributes[i].Match(other[i])) return false;
				}
				return true;
			}
		}
	    
        private sealed class FieldPropertyDescriptor : PropertyDescriptor
        {
            private readonly FieldInfo _field;

            public FieldPropertyDescriptor(FieldInfo field) : 
                base(field.Name, (Attribute[]) field.GetCustomAttributes(typeof(Attribute), true))
            {
                _field = field;
            }

            public override bool Equals(object obj)
            {
                FieldPropertyDescriptor other = obj as FieldPropertyDescriptor;
                return other != null && other._field.Equals(_field);
            }

            public override int GetHashCode() { return _field.GetHashCode(); }

            public override bool IsReadOnly { get { return false; } }
            public override void ResetValue(object component) {}
            public override bool CanResetValue(object component) { return false; }
            public override bool ShouldSerializeValue(object component) { return true; }

            public override Type ComponentType { get { return _field.DeclaringType; } }
            public override Type PropertyType { get { return _field.FieldType; } }

            public override object GetValue(object component) { return _field.GetValue(component); }

            public override void SetValue(object component, object value) 
            {
                _field.SetValue(component, value); 
                OnValueChanged(component, EventArgs.Empty);
            }
        }
	}
}
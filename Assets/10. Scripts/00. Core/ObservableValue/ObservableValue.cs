using System;
using System.Collections.Generic;
using UnityEngine;

namespace jjudy
{
    public class ObservableValue<T> where T : struct
    {
        private T value;

        /// <summary>
        /// prev, current
        /// </summary>
        public event Action<T, T> OnValueChanged;

        public T Value
        {
            get => value;
            set
            {
                T oldValue = this.value;
                
                if (EqualityComparer<T>.Default.Equals(oldValue, value))
                    return;

                this.value = value;
                OnValueChanged?.Invoke(oldValue, this.value);
            }
        }
        
        public ObservableValue() { }

        public ObservableValue(T initialValue)
        {
            value = initialValue;
        }
    }
}



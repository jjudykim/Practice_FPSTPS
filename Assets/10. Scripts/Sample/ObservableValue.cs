using System;
using UnityEngine;

namespace KooHoo
{
    public class ObservableValue<T> where T : struct
    {
        private T value;

        /// <summary>
        /// prev, current
        /// </summary>
        public Action<T, T> OnValueChanged;

        public T Value
        {
            get => value;
            set
            {
                T  oldValue = this.value;
                this.value = value;
                OnValueChanged?.Invoke(oldValue, this.value);
            }
        }
    }
}


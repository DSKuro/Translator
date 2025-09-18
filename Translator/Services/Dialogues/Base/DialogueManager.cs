using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace Translator.Services.Dialogues.Base
{
    public class DialogueManager : IDialogueManager
    {
        private static readonly Dictionary<object, Visual> RegistrationMapper =
            new Dictionary<object, Visual>();

        public static readonly AttachedProperty<object?> RegisterProperty =
            AvaloniaProperty.RegisterAttached<DialogueManager, Visual, object?>("Register");

        public static void SetRegister(AvaloniaObject element, object value)
        {
            element.SetValue(RegisterProperty, value);
        }

        public static object? GetRegister(AvaloniaObject element)
        {
            return element.GetValue(RegisterProperty);
        }

        static DialogueManager()
        {
            RegisterProperty.Changed.AddClassHandler<Visual>(RegisterChanged);
        }

        private static void RegisterChanged(Visual sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is null)
            {
                throw new InvalidOperationException("The DialogueManager can only be registered on a Visual");
            }

            RegisterChangedImpl(sender, e);
        }

        private static void RegisterChangedImpl(Visual sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && RegistrationMapper.ContainsKey(e.OldValue))
            {
                RegistrationMapper.Remove(e.OldValue);
            }

            if (e.NewValue != null)
            {
                RegistrationMapper[e.NewValue] = sender;
            }
        }

        public Visual? GetVisualForContext(object context)
        {
            return RegistrationMapper.TryGetValue(context, out Visual? result) ? result : null;
        }

        public TopLevel? GetTopLevelForContext(object context)
        {
            Visual? visual = GetVisualForContext(context);
            return TopLevel.GetTopLevel(visual);
        }
    }
}

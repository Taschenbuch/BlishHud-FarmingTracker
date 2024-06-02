using Blish_HUD.Controls;
using System;
using System.Reflection;

namespace FarmingTracker
{
    public class Hacks
    {
        public static void ClearAndAddChildrenWithoutUiFlickering(ControlCollection<Control> children, Container parent)
        {
            try            
            {
                var oldChildren = parent.Children;

                foreach (var item in children)
                    GetPrivateField(item, "_parent").SetValue(item, parent); // because .Parent will otherwise trigger UI Update

                GetPrivateField(parent, "_children").SetValue(parent, children);
                parent.Invalidate();

                foreach (var oldChild in oldChildren)
                    oldChild.Dispose(); // called at the end and inside try-catch because when called at the beginning or after a failed try-catch it triggers an enumeration was modified exception.
            }
            catch (Exception e)
            {
                Module.Logger.Error(e, "Failed to add children to container with reflection");
            }
        }

        private static FieldInfo GetPrivateField(object target, string fieldName)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "The assignment target cannot be null.");
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("The field name cannot be null or empty.", nameof(fieldName));
            }

            var t = target.GetType();

            const BindingFlags BF = BindingFlags.Instance |
                                    BindingFlags.NonPublic |
                                    BindingFlags.DeclaredOnly;

            FieldInfo fi;

            while ((fi = t.GetField(fieldName, BF)) == null && (t = t.BaseType) != null)
            {
                /* NOOP */
            }

            return fi;
        }
    }
}

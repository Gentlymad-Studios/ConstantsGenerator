using System;
using System.Collections.Generic;

namespace ConstantsGenerator {
    public interface ILookupModel {
        public string GetComment(ref string value);
        public string GetTypeName();
        public void SetTransformAction(Action<string, string, string> transform);
        public void TransformEach();
        public void Cleanup(ref string key, ref string value);
    }

    public abstract class LookupModel : ILookupModel {
        public Action<string, string, string> transform;

        protected void DefaultCleanup(ref string key, ref string value) {
            value = value.Replace(' ', '_');
        }

        public virtual void Cleanup(ref string key, ref string value) {
            DefaultCleanup(ref key, ref value);
        }

        public void SetTransformAction(Action<string, string, string> transform) {
            this.transform = transform;
        }

        public abstract void TransformEach();

        public abstract string GetTypeName();

        public abstract string GetComment(ref string fieldName);

    }

    public class LookupModel<T> : LookupModel {
        public Dictionary<string, string> commentLookup = new Dictionary<string, string>();
        public Dictionary<T, string> lookup = new Dictionary<T, string>();
        public bool hasComments = false;

        public override string GetComment(ref string value) {
            if(hasComments && commentLookup.ContainsKey(value)) {
                return commentLookup[value];
            }
            return null;
        }

        public override string GetTypeName() {
            return typeof(T).ToString();
        }

        public override void TransformEach() {
            string key, value, comment;

            foreach (KeyValuePair<T, string> keyValuePair in lookup) {
                key = keyValuePair.Key.ToString();
                value = keyValuePair.Value;
                Cleanup(ref key, ref value);
                comment = GetComment(ref value);
                transform.Invoke(key, value, comment);
            }
        }
    }

    public class IntLookupModel : LookupModel<int> {
        public override string GetTypeName() => "int";
    }
    public class StringLookupModel : LookupModel<string> {
        public override string GetTypeName() => "string";
    }
    public class FloatLookupModel : LookupModel<float> {
        public override string GetTypeName() => "float";
    }
    public class BoolLookupModel : LookupModel<bool> {
        public override string GetTypeName() => "bool";
    }

}
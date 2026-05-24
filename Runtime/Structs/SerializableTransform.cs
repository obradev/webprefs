using UnityEngine;

namespace ObraDev.WebPrefs
{
    public struct SerializableTransform
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public SerializableTransform(Transform transform)
        {
            position = transform.position;
            rotation = transform.eulerAngles;
            scale = transform.localScale;
        }

        public SerializableTransform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
        
        public SerializableTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation.eulerAngles;
            this.scale = scale;
        }
        
        public SerializableTransform(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = Vector3.one;
        } 

        public SerializableTransform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation.eulerAngles;
            this.scale = Vector3.one;
        }
        
        public SerializableTransform(Vector3 position)
        {
            this.position = position;
            this.rotation = Vector3.zero;
            this.scale = Vector3.one;
        }
        
        public void ApplyTo(Transform transform)
        {
            transform.position = position;
            transform.eulerAngles = rotation;
            transform.localScale = scale;
        }
        
        public override string ToString()
        {
            return "P: " + position + " R: " + rotation + " S: " + scale;
        }
    }
}
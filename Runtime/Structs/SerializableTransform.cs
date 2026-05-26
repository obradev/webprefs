using UnityEngine;

namespace ObraDev.WebPrefs
{
    /// <summary>
    /// A serializable snapshot of a Transform's position, rotation (euler angles) and scale.
    /// Can be saved and loaded with WebPrefs.Save() and WebPrefs.LoadSTransform().
    /// </summary>
    /// <example>
    /// <code>
    /// // Save
    /// WebPrefs.Save("player", new SerializableTransform(transform));
    /// 
    /// // Load and apply
    /// WebPrefs.LoadSTransform("player").ApplyTo(transform);
    /// </code>
    /// </example>
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
        
        /// <summary>Applies the saved position, rotation and scale back to a Transform.</summary>
        public void ApplyTo(Transform transform)
        {
            transform.position = position;
            transform.eulerAngles = rotation;
            transform.localScale = scale;
        }
        
        /// <summary>Outputs the saved position, rotation and scale as a string.</summary>
        public override string ToString()
        {
            return "P: " + position + " R: " + rotation + " S: " + scale;
        }
    }
}

using NLog;
using UnityEngine;

namespace TM
{
    public class ParentManager : MonoBehaviour
    {
        public static ParentManager Instance;
        private ObjectController _selectedObjectController;
        private ParentCommand _parentCommand;

        public ParentCommand ParentCommand
        {
            get { return _parentCommand; }
            set { SetNewParentCommand(value); }
        }

        private void SetNewParentCommand(ParentCommand value)
        {
            _parentCommand = value;

            switch (ParentCommand)
            {
                case ParentCommand.DeleteParent:
                    break;
                case ParentCommand.SetNew:

                    break;
            }
        }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public ObjectController GetSelectedParent()
        {
            return _selectedObjectController;
        }

        public void SetSelectedBaseType(ObjectController selected)
        {
            _selectedObjectController = selected;
        }

        public void Invoke(ObjectController newParent)
        {
            switch (ParentCommand)
            {
                case ParentCommand.SetThis:
                    ParentCommand = ParentCommand.None;
                    break;
            }
            
        }
    }

    public enum ParentCommand
    {
        None, SetThis, SetNew, DeleteParent
    }
}

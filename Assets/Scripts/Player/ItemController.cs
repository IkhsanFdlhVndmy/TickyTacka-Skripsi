using UnityEngine;

namespace Player
{
    public class ItemController:MonoBehaviour
    {
        private Animator _animator;
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }
        public void PlayExplodeAnim()
        {
            _animator.SetTrigger("Explode");
        }

    }
}
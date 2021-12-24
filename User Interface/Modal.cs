using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

using Sneaksters.UI.Menus;
using Sneaksters.UI.Animation;

namespace Sneaksters.UI
{
    public class Modal : MonoBehaviour
    {
        // TO DO:
        // * Manage the animation/etc of the indeterminate progress indicator
        //   and also maybe work on putting other images in the modal? idk
        

        [SerializeField]
        private ModalAnimator animator = null;

        [SerializeField]
        private IndeterminateProgressIndicator indeterminateProgressIndicator = null;

        [SerializeField]
        private Button dismissButton = null, yesButton = null, noButton = null;

        [SerializeField]
        private SimpleLocalizeText bodyTopText = null, bodyBottomUpperText = null, bodyBottomLowerText = null, dismissButtonText = null, yesButtonText = null, noButtonText = null;


        // Delegates
        public delegate void ModalEvent();

        void Start() {
            animator.SetActive(false);
        }

        public void ShowConnecting(string bodyTextID, ModalEvent onAppear) {
            if (onAppear == null) onAppear = () => { };

            // There's no buttons here, so only update the body text
            bodyBottomLowerText?.SetTextID(bodyTextID);
            bodyBottomUpperText?.SetTextID("");
            bodyTopText?.SetTextID("");

            // Disable the buttons
            dismissButton?.gameObject.SetActive(false);
            yesButton?.gameObject.SetActive(false);
            noButton?.gameObject.SetActive(false);

            // Enable the indeterminate progress indicator animator
            indeterminateProgressIndicator?.Activate();

            // Clear all button listeners - probably not necessary but nice anyways
            ClearAllButtonListeners();

            // Animate modal box in
            AnimateModalIn(onAppear);
        }

        public void ShowError(string bodyTextID, string dismissButtonTextID, bool makeDismissButtonVisible, ModalEvent onAppear, ModalEvent onConfirm) {
            if (onAppear == null) onAppear = () => { };
            if (onConfirm == null) onConfirm = () => Hide();

            // Update the body text as well as the "Ok" button text
            bodyBottomUpperText?.SetTextID(bodyTextID);
            bodyBottomLowerText?.SetTextID("");
            bodyTopText?.SetTextID("");
            dismissButtonText?.SetTextID(dismissButtonTextID);

            // Only enable the error button
            dismissButton?.gameObject.SetActive(makeDismissButtonVisible);
            yesButton?.gameObject.SetActive(false);
            noButton?.gameObject.SetActive(false);

            // Disable the indeterminate progress indicator animator
            indeterminateProgressIndicator?.Deactivate();

            // Select the dismiss button if necessary
            if (makeDismissButtonVisible) dismissButton?.Select();
            if (MainMenuManager.instance != null)
                MainMenuManager.instance.lastSelectedGameObject = dismissButton?.gameObject;

            // Clear all button listeners
            ClearAllButtonListeners();

            // Set the button listener for the error button
            dismissButton?.onClick.AddListener(new UnityAction(onConfirm));

            // Animate modal box in
            AnimateModalIn(onAppear);
        }

        public void ShowYesNo(string bodyTextID, string yesTextID, string noTextID, ModalEvent onAppear, ModalEvent onYes, ModalEvent onNo) {
            if (onAppear == null) onAppear = () => { };
            if (onYes == null) onYes = () => { };
            if(onNo == null) onNo = () => { };

            // Update the body text as well as the "Yes" and "No" button text
            bodyBottomUpperText?.SetTextID(bodyTextID);
            bodyBottomLowerText?.SetTextID("");
            bodyTopText?.SetTextID("");

            yesButtonText?.SetTextID(yesTextID);
            noButtonText?.SetTextID(noTextID);

            // Only enable the ok and no buttons
            dismissButton?.gameObject.SetActive(false);
            yesButton?.gameObject.SetActive(true);
            noButton?.gameObject.SetActive(true);

            // Disable the indeterminate progress indicator animator
            indeterminateProgressIndicator?.Deactivate();

            // Clear all button listeners
            ClearAllButtonListeners();

            // Select the deny button by default
            noButton?.Select();
            if (MainMenuManager.instance != null)
                MainMenuManager.instance.lastSelectedGameObject = noButton?.gameObject;

            // Set the button listener for the yes and no buttons
            yesButton?.onClick.AddListener(new UnityAction(onYes));
            noButton?.onClick.AddListener(new UnityAction(onNo));

            // Animate modal box in
            AnimateModalIn(onAppear);
        }

        void ClearAllButtonListeners() {
            dismissButton?.onClick.RemoveAllListeners();
            yesButton?.onClick.RemoveAllListeners();
            noButton?.onClick.RemoveAllListeners();
        }

        void AnimateModalIn(ModalEvent onAppear) {
            // Animate the modal box in, and run the onAppear method once it's in

            if (onAppear == null) onAppear = () => {};
            animator?.AnimateModalIn(new TweenCallback(onAppear));
        }

        public void Hide(ModalEvent onDisappear = null) {
            // Animate the modal box out, and run the onDisappear method once it's out
            if (onDisappear == null) onDisappear = () => {};
            animator?.AnimateModalOut(new TweenCallback(onDisappear));
        }



    }

    public enum ModalMode {
        Connecting = 0,
        Error = 1,
        YesNo = 2
    }
}
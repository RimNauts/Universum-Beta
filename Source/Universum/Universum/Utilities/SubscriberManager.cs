using System;
using Verse;

namespace Universum.Utilities {
    public class SubscriberManager {
        private int _numSubscribers = 0;
        private bool _settingsEnabled = true;

        public bool Active { get; private set; } = false;

        public SubscriberManager() {
            Deactivate();
        }

        public void Subscribe() {
            _numSubscribers++;

            UpdateActiveState();
        }

        public void Unsubscribe() {
            if (_numSubscribers <= 0) return;

            _numSubscribers--;

            UpdateActiveState();
        }

        public void Reset() {
            _numSubscribers = 0;

            UpdateActiveState();
        }

        public void SetSettingsEnabled(bool enabled) {
            if (_settingsEnabled == enabled) return;

            _settingsEnabled = enabled;

            UpdateActiveState();
        }

        private void UpdateActiveState() {
            bool shouldBeActive = _numSubscribers > 0 && _settingsEnabled;

            if (Active == shouldBeActive) return;

            Active = shouldBeActive;
            if (Active) {
                Activate();
            } else {
                Deactivate();
            }
        }

        private void Activate() {
            Log.Message("Activated: Harmony patches are now active.");
        }

        private void Deactivate() {
            Log.Message("Deactivated: Harmony patches are now inactive.");
        }
    }
}

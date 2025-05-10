namespace UnityEngine.XR.ARCore.Tests
{
    class CodeExamples
    {
        #region ArRecordingConfig_example

        void RecordExample(ARCoreSessionSubsystem subsystem, string mp4PathUrl)
        {
            var session = subsystem.session;
            using (var config = new ArRecordingConfig(session))
            {
                config.SetMp4DatasetUri(session, mp4PathUrl);
                config.SetRecordingRotation(session, 90);
                config.SetAutoStopOnPause(session, false);
                var status = subsystem.StartRecording(config);
                Debug.Log($"StartRecording to {config.GetMp4DatasetUri(session)} => {status}");
            }
        }
        #endregion
    }
}

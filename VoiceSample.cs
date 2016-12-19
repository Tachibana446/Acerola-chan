using Discord;
using Discord.Audio;
using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SinobigamiBot
{
    class VoiceSample
    {
        private DiscordClient Client { get; set; }
        public IAudioClient VoiceClient { get; set; }

        public VoiceSample(DiscordClient client)
        {
            Client = client;
        }

        /// <summary>
        /// これを呼び出して使う
        /// </summary>
        /// <returns></returns>
        public async Task SendAudio(Channel vChannel, string filepath)
        {
            await JoinChannel(vChannel);
            SendAudio(filepath);
        }

        private async Task JoinChannel(Channel vChannel)
        {
            VoiceClient = await Client.GetService<AudioService>().Join(vChannel);

        }

        private void SendAudio(string filepath, float volume = 1)
        {
            if (!System.IO.File.Exists(filepath))
                throw new Exception("not found!!!!" + filepath);

            var channelCount = Client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
            var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using (var audioFileReader = new AudioFileReader(filepath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using (var resampler = new MediaFoundationResampler(audioFileReader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
                audioFileReader.Volume = volume;
                resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                byte[] buffer = new byte[blockSize];
                int byteCount;

                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                {
                    if (byteCount < blockSize)
                    {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }
                    VoiceClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                }
            }

        }
    }
}

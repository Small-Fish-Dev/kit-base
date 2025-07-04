using System.Text.Json.Nodes;
using Sandbox.Audio;

namespace GameFish;

public partial class Settings : DataFile<Settings.Data>
{
	public override string FileName => "settings.cfg";

	public partial class Data
	{
		public Dictionary<string, float> Volumes { get; set; } = GetMixerVolumes();

		public static Dictionary<string, float> GetMixerVolumes()
		{
			var mixerVolumes = new Dictionary<string, float>();

			void SetVolume( string name, float volume )
			{
				if ( !string.IsNullOrEmpty( name ) )
					mixerVolumes[name] = volume;
			}

			if ( Mixer.Master is not null )
				SetVolume( Mixer.Master.Name ?? "Master", Mixer.Master.Volume );

			foreach ( var mixer in Mixer.Master.GetChildren() )
				SetVolume( mixer.Name, mixer.Volume );

			return mixerVolumes;
		}
	}
}

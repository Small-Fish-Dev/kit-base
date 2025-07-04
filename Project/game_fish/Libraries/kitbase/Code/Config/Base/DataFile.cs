using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameFish;

/// <summary>
/// ⚙ 🛠 📋 <br />
/// A component for managing a specific file. <br />
/// Can read/write any type of JSON-serializable data.
/// </summary>
[Icon( "save" )]
public abstract partial class DataFile<TData> : Component
{
	public const string GROUP_FILE = "💾 File";
	public const string GROUP_DATA = "🔍 Data";
	public const string GROUP_DEBUG = "🐞 Debug";

	private static DataFile<TData> _instance;
	public static DataFile<TData> Instance
	{
		get => _instance.GetSingleton( allowEditor: true );
		set => _instance = value;
	}

	[JsonIgnore]
	[Property, Group( GROUP_DATA ), Title( "Data" ), Order( 42069 )]
	public Dictionary<string, JsonNode> Entries => DataObject?.ToDictionary();
	public JsonObject DataObject { get; set; }

	[JsonIgnore]
	[Property, ReadOnly]
	public abstract string FileName { get; }

	public static string Folder => Game.Ident;
	public static string FilePath => $"{Folder}/{Instance?.FileName ?? "error.gamefish"}";

	public JsonWriterOptions WriteOptions { get; set; } = new()
	{
		Indented = true,
	};

	/// <summary>
	/// If the file data has been modified in some way since saving.
	/// </summary>
	[Property, ReadOnly, Group( GROUP_FILE )]
	public bool IsDirty { get; protected set; }

	/// <summary>
	/// Load the file whenever this component is loaded?
	/// </summary>
	[Property, Group( GROUP_FILE )]
	public bool AutoLoading { get; set; } = true;

	/// <summary>
	/// Automatically save upon a change after <see cref="AutoSaveDelay"/>?
	/// </summary>
	[Property, Group( GROUP_FILE )]
	public bool AutoSaving { get; set; } = true;

	/// <summary>
	/// If the file data has been modified in some way since saving.
	/// </summary>
	[Range( 5f, 60f, step: 1f, clamped: false )]
	[Property, Group( GROUP_FILE )]
	public float AutoSaveDelay { get; set; } = 30f;

	[Property, Group( GROUP_FILE ), Title( "Next Auto-Save" )]
	public double? AutoSaveTimer => IsDirty ? NextAutoSave.Relative : null;
	public RealTimeUntil NextAutoSave { get; protected set; }

	/// <summary> Print debug logs? </summary>
	[Property, Group( GROUP_DEBUG ), Title( "Debug Logging" )]
	public bool Debug { get; set; } = false;

	[JsonIgnore]
	[Property, Group( GROUP_DEBUG ), Title( "Key" )]
	public string SetKey { get; set; }

	[JsonIgnore]
	[Property, Group( GROUP_DEBUG ), Title( "Type" )]
	public Type SetType { get; set; }

	[JsonIgnore]
	[Property, Group( GROUP_DEBUG ), Title( "Value" )]
	public string SetValue { get; set; }

	[Button, Group( GROUP_DEBUG ), Title( "Set Value" )]
	public void DebugSet()
	{
		try
		{
			Set( SetKey, Json.Deserialize( SetValue, SetType ) );
		}
		catch ( Exception e )
		{
			this.Warn( e );
		}
	}

	/// <returns> The file's type and file path. </returns>
	public override string ToString()
		=> $"{this?.GetType().ToSimpleString()}|\"{this?.FileName}\"";

	protected override Task OnLoad()
	{
		if ( !(Scene?.IsEditor ?? true) )
			TryLoad();

		return base.OnLoad();
	}

	/// <returns> A new instance of <typeparamref name="TData"/>. </returns>
	protected TData CreateData()
		=> TypeLibrary.Create<TData>( type: typeof( TData ) );

	[Button, Group( GROUP_FILE ), Title( "Load" )]
	public bool TryLoad()
	{
		DebugLog( "Trying to load." );

		if ( string.IsNullOrWhiteSpace( FileName ) )
		{
			this.Warn( $"{nameof( FileName )} property was missing/invalid" );
			return false;
		}

		try
		{
			FileSystem.OrganizationData.CreateDirectory( Folder );
			var fileString = FileSystem.OrganizationData.ReadAllText( FilePath );

			if ( string.IsNullOrWhiteSpace( fileString ) )
			{
				DebugLog( $"File was missing/empty. Attempting to initialize." );

				DataObject = JsonSerializer.SerializeToNode( CreateData() ) as JsonObject;

				if ( DataObject is not null )
				{
					this.Log( $"Initialized data." );
					return true;
				}

				this.Warn( $"Failed to initialize data. Result: null." );
				return false;
			}

			var data = CreateData();
			var objParsed = Json.ParseToJsonObject( fileString );

			if ( objParsed is not null )
			{
				var propertyTable = TypeLibrary.GetPropertyDescriptions( data, onlyOwn: true );

				foreach ( var kv in objParsed )
				{
					var p = propertyTable.FirstOrDefault( p => p?.Name == kv.Key );

					if ( p is null )
					{
						this.Log( $"file had unhandled member \"{kv.Key}\"" );
						continue;
					}

					if ( Json.TryDeserialize( kv.Value.ToString(), p.PropertyType, out var val ) )
						TypeLibrary.SetProperty( data, p.Name, val );
					else
						this.Warn( $"failed to deserialize {kv.Key} as {p.PropertyType}" );
				}
			}

			var obj = JsonSerializer.SerializeToNode( data ) as JsonObject;

			if ( obj is not null )
			{
				DataObject?.Clear();
				DataObject = obj;

				DebugLog( "Successfully loaded." );

				return true;
			}
		}
		catch ( Exception e )
		{
			this.Warn( "LOADING EXCEPTION: " + e );

			return false;
		}

		this.Warn( "Failed to load." );

		return false;
	}

	[Button, Group( GROUP_FILE ), Title( "Save" )]
	public bool TrySave()
	{
		IsDirty = false;

		DebugLog( "Trying to save." );

		if ( string.IsNullOrWhiteSpace( FileName ) )
		{
			this.Warn( $"{nameof( FileName )} property was missing/invalid" );
			return false;
		}

		try
		{
			if ( DataObject is null )
			{
				this.Log( "Nothing to save." );
				return false;
			}

			var str = DataObject.ToJsonString();

			if ( string.IsNullOrWhiteSpace( str ) )
			{
				this.Warn( "Empty JSON string." );
				return false;
			}

			FileSystem.OrganizationData.WriteAllText( FilePath, str );

			DebugLog( "Successfully saved." );

			return true;
		}
		catch ( Exception e )
		{
			this.Warn( "SAVING EXCEPTION: " + e );

			return false;
		}
	}

	public static bool TrySet<T>( string name, T value )
		=> Instance?.Set( name, value ) ?? false;

	public static bool TryGet<T>( string name, out T value, T defaultValue = default )
		=> Instance?.Get( name, out value, defaultValue ) ?? (value = default) is true;

	protected bool Set<T>( string name, T value )
	{
		if ( DataObject is null )
			if ( !TryLoad() )
				return false;

		if ( DataObject is null )
			return false;

		try
		{
			DataObject[name] = Json.ToNode( value );

			if ( !IsDirty )
			{
				IsDirty = true;
				NextAutoSave = AutoSaveDelay;
			}

			DebugLog( $"Set \"{name}\" to \"{value}\"" );
		}
		catch
		{
			this.Warn( $"Exception when setting \"{name ?? "null"}\" to \"{value?.ToString() ?? "null"}\"" );

			return false;
		}

		return true;
	}

	protected bool Get<T>( string name, out T value, T defaultValue = default )
	{
		if ( DataObject is null )
		{
			if ( !TryLoad() )
			{
				value = defaultValue;
				return false;
			}
		}

		if ( DataObject is null )
		{
			value = defaultValue;
			return false;
		}

		try
		{
			value = DataObject.GetPropertyValue<T>( name, defaultValue );
		}
		catch
		{
			this.Warn( $"Exception when getting \"{name}\" as {typeof( T )}" );

			value = defaultValue;

			return false;
		}

		return true;
	}

	public bool AutoSave( bool force = false )
	{
		if ( !force && (!IsDirty || !NextAutoSave) )
			return false;

		if ( !TrySave() )
			return false;

		this.Log( "Auto-save successful." );

		return true;
	}

	protected void DebugLog( params object[] log )
	{
		if ( Debug )
			this.Log( log );
	}
}

namespace GameFish;

partial class ActorCitizenModel
{
	public const string GROUP_DRESS = "👗 Cosmetics";

	[Property, Feature( Actor.FEATURE_ACTOR ), Group( GROUP_DRESS )]
	public bool UseLocalClothing { get; set; }

	[Property, Feature( Actor.FEATURE_ACTOR ), Group( GROUP_DRESS )]
	public bool UseCustomClothing { get; set; } = true;

	[ShowIf( nameof( UseCustomClothing ), true )]
	[Property, Feature( Actor.FEATURE_ACTOR ), Group( GROUP_DRESS )]
	public List<Clothing> Clothing { get => _clothing; set { _clothing = value; } }
	private List<Clothing> _clothing;

	public ClothingContainer ClothingContainer { get; set; }

	[Button, Feature( Actor.FEATURE_ACTOR ), Group( GROUP_DRESS )]
	public virtual void Dress()
	{
		ClothingContainer ??= new();
		ClothingContainer.Clothing?.Clear();
		ClothingContainer.PrefersHuman = true;

		Clothing ??= [];

		// Allow avatar customization.
		if ( UseLocalClothing )
		{
			var userClothing = ClothingContainer.CreateFromLocalUser()?.Clothing ?? [];

			foreach ( var clothing in userClothing )
				if ( clothing is not null )
					ClothingContainer.Add( clothing );
		}

		// Allow customized clothing(overrides avatar).
		if ( UseCustomClothing )
		{
			foreach ( var clothing in Clothing )
				if ( clothing is not null )
					ClothingContainer.Add( clothing );

			if ( SkinnedModel.IsValid() )
				ClothingContainer.Apply( SkinnedModel );
		}
	}
}

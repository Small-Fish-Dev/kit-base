namespace GameFish;

partial class Server
{
	[Property, ReadOnly]
	public List<Pawn> AllPawns => Pawn.GetAll<Pawn>().ToList();

	[Property, ReadOnly]
	public List<Pawn> ActivePawns => Pawn.GetAllActive<Pawn>().ToList();
}

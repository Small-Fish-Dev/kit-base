namespace GameFish;

partial class Server
{
	[Property, ReadOnly]
	public List<BasePawn> AllPawns => BasePawn.GetAll<BasePawn>().ToList();

	[Property, ReadOnly]
	public List<BasePawn> ActivePawns => BasePawn.GetAllActive<BasePawn>().ToList();
}

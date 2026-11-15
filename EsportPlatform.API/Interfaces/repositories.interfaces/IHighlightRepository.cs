


using EsportPlatform.API.Models;

namespace EsportPlatform.API.Interfaces.repositories.interfaces;

public interface IHighlightRepository:IRepository<Highlight>
{
   
    Task<bool> ExistByGameIdAsync(int gameid);
}
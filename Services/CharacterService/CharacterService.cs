using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    { 
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public CharacterService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = _mapper.Map<Character>(newCharacter);
            
            _context.Characters.Add(character);
            await _context.SaveChangesAsync();
            serviceResponse.Data = 
                await _context.Characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            
            try {
            var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id);
            if(character is null)
            throw new Exception($"Character with Id '{id}' not Found");
            
            _context.Characters.Remove(character);

            await _context.SaveChangesAsync();
            serviceResponse.Data = 
                await _context.Characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            
            } catch(Exception ex){
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _context.Characters.ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {   
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbcharacter = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id);
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbcharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            
            try {
            var character = 
                await _context.Characters.FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
            if(character is null)
            throw new Exception($"Character with Id '{updatedCharacter.Id}' not Found");
            
            
            _mapper.Map(updatedCharacter,character);

            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            
            } catch(Exception ex){
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}
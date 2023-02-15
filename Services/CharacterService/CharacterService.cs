using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    { 
        private static List<Character> characters = new List<Character>{
            new Character(),   
            new Character{Id = 1, Name = "Sam"}
        };
        private readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = _mapper.Map<Character>(newCharacter);
            character.Id = characters.Max(c => c.Id) + 1;
            characters.Add(character);
            characters.Add(_mapper.Map<Character>(newCharacter));
            serviceResponse.Data = characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            
            try {
            var character = characters.FirstOrDefault(c => c.Id == id);
            if(character is null)
            throw new Exception($"Character with Id '{id}' not Found");
            
            characters.Remove(character);

            serviceResponse.Data = characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            
            } catch(Exception ex){
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            serviceResponse.Data = characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {   
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = characters.FirstOrDefault(c => c.Id == id);
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            
            try {
            var character = characters.FirstOrDefault(c => c.Id == updatedCharacter.Id);
            if(character is null)
            throw new Exception($"Character with Id '{updatedCharacter.Id}' not Found");
            
            _mapper.Map(updatedCharacter,character);

            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            
            } catch(Exception ex){
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}
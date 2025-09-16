//using AutoMapper;
//using Microsoft.Extensions.DependencyInjection;
//using src.DTOs.Response;
//using src.Entities;
//using src.Helper.AutoMapper;

//namespace src.Helper
//{
//    public class MapperConfig : IMapperConfig
//    {
//        private readonly ILoggerFactory _loggerFactory;
//        public MapperConfig(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

//        public IMapper InitializeAutomapper()
//        {
//            var config = new MapperConfiguration(cfg =>
//            {
//                cfg.CreateMap<Tuition, GetTuitionResponseDto>();
//            }, _loggerFactory);

//            return config.CreateMapper();
//        }
//    }

//}

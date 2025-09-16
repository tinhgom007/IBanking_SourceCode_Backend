//using AutoMapper;
//using src.Helper.AutoMapper;

//namespace src.Helper
//{
//    public class AutoMap : MapperConfig, IAutoMap
//    {
//        private Mapper _mapper;
//        public AutoMap(Mapper mapper)
//        {
//            _mapper = mapper;
//        }

//        public TOutput Map<TInput, TOutput>(TInput inputObj)
//        {
//            try
//            {
//                return _mapper.Map<TInput, TOutput>(inputObj);
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
//    }
//}

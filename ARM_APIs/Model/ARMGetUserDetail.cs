//using ARM_APIs.Interface;
//using ARMCommon.Helpers;
//using ARMCommon.Interface;
//using ARMCommon.Model;
//using Microsoft.EntityFrameworkCore;

//namespace ARM_APIs.Model
//{
//    public class ARMGetUserDetail : IARMGetUserDetail
//    {

//        private readonly DataContext _context;
//        private readonly IConfiguration _config;
//        private readonly IRedisHelper _redis;
//        private readonly ITokenService _tokenService;

//        public ARMGetUserDetail(DataContext context, IConfiguration configuration, ITokenService tokenService, IRedisHelper redis)
//        {
//            _context = context;
//            _config = configuration;
//            _tokenService = tokenService;
//            _redis = redis;
//        }
//        public async Task<ARMUser> GetUserProfileDetails(string ARMSessionId)
//        {
//            var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
//            var loginuser = await GetUserProfile(dictSession["APPNAME"], dictSession["USERNAME"], dictSession["USERGROUP"]);
//            return loginuser;
//        }

  
//        private async Task<ARMUser> GetUserProfile(string appName, string username, string usergroup)
//        {
//            return await _context.ARMUsers.FirstOrDefaultAsync(f => f.username.ToLower() == username.ToLower() && f.appname.ToLower() == appName.ToLower() && f.usergroup.ToLower() == usergroup.ToLower());

//        }

      
//        public async Task<bool> UpdateUserProfileDetails(string ARMSessionId, string email, string mobileno)
//        {
//            var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
//            var loginuser = await GetUserProfile(dictSession["APPNAME"], dictSession["USERNAME"], dictSession["USERGROUP"]);
//            loginuser.email = email;
//            loginuser.mobileno = mobileno;
//            _context.Update(loginuser);
//            var updated = await _context.SaveChangesAsync();
//            if (updated > 0)
//            {
//                return true;
//            }

//            else
//            {
//                return false;
//            }
//        }


//        public async Task<bool> MobileExist(string mobile)
//        {
//         var user =  await _context.ARMUsers.FirstOrDefaultAsync(f => f.mobileno.ToLower() == mobile);
//            if (user == null)
//            {
//                return false;
//            }
//            else
//            {
//                return true;
//            }
//        }
//    }
//}

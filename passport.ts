////import passport from 'passport';
//import { Strategy as GoogleStrategy } from 'passport-google-oauth20';

//passport.use(
//    new GoogleStrategy(
//        {
//            clientID: process.env.GOOGLE_CLIENT_ID!,
//            clientSecret: process.env.GOOGLE_CLIENT_SECRET!,
//            callbackURL: '/auth/google/callback',
//        },
//        (accessToken, refreshToken, profile, done) => {
//            // Handle user profile information
//            console.log(profile);
//            done(null, profile);
//        }
//    )
//);

//// Serialize and deserialize user
//passport.serializeUser((user, done) => {
//    done(null, user);
//});

//passport.deserializeUser((user: Express.User, done) => {
//    done(null, user);
//});

//export default passport;

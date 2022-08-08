#import "UnityAppController.h"
#import <UniversalLink.h> 
@interface BloctoAppController : UnityAppController
@end
 
IMPL_APP_CONTROLLER_SUBCLASS (BloctoAppController)
 
@implementation BloctoAppController
 
- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    return YES;
}
 
- (void)onOpenURL:(NSNotification *)notification{
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"UIAlertView"
            message:@"In notification" delegate:self cancelButtonTitle:@"Cancel"
            otherButtonTitles:@"OK", nil];
    
    [alert show];
}


- (BOOL)application:(UIApplication *)application continueUserActivity:(nonnull NSUserActivity *)userActivity restorationHandler:(nonnull void (^)(NSArray<id<UIUserActivityRestoring>> *_Nullable))restorationHandler {
    
    if ([userActivity.activityType isEqualToString: NSUserActivityTypeBrowsingWeb]) {
        NSURL *url = userActivity.webpageURL;
        if ([url.query containsString:@"request_id"]) {
            NSString *query = [url query];
            [UniversalLink instance].URL = query;
            return YES;
        }
    }
    
    return YES;
}
@end

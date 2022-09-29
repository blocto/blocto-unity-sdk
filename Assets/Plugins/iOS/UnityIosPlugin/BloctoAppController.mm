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


- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray<id<UIUserActivityRestoring>> *_Nullable))restorationHandler {
   
    
    if ([userActivity.activityType isEqualToString: NSUserActivityTypeBrowsingWeb]) {
        NSURL *url = userActivity.webpageURL;
        if ([url.query containsString:@"request_id"]) {
            NSString *query = [url query];
            [UniversalLink instance].URL = query;

            NSLog(@"Universal link: %@", [UniversalLink instance].URL);
            UniversalLink_GetURL();
            UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"UIAlertView"
            message:query delegate:self cancelButtonTitle:@"Cancel"
            otherButtonTitles:@"OK", nil];
            [alert show];

            return YES;
        }
    }
    
    return YES;
}


- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"UIAlertView"
            message:@"In notification" delegate:self cancelButtonTitle:@"Cancel"
            otherButtonTitles:@"OK", nil];
    
    [alert show];
    return YES;
}

- (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url
{
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"UIAlertView"
            message:@"In notification" delegate:self cancelButtonTitle:@"Cancel"
            otherButtonTitles:@"OK", nil];
    
    [alert show];
    return YES;
}

@end

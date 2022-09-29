#import <Foundation/Foundation.h>
#import <UniversalLink.h>

__strong UniversalLink *_instance;
@implementation UniversalLink
+(void)load
{
    NSLog(@"[UniversalLink load]");
    _instance = [UniversalLink new];
}
+(UniversalLink*)instance
{ return _instance; }

-(instancetype)init
{
    self = [super init];
    if (self) [self reset];
    return self;
}

-(void)reset
{
    //self.URL = [NSString new];
    NSLog(@"UniversalLink Reset.");
    self.URL = [NSString stringWithUTF8String:""];;
}

-(NSString*)getUrl
{
    return self.URL;
}
@end

extern "C"
{
    
    void UniversalLink_Reset()
    { return [[UniversalLink instance] reset]; }

    const char* UniversalLink_GetURL()
    {
        try
        {
            NSLog(@"UniversalLink GetURL");
            const char* cString = [[UniversalLink instance] URL].UTF8String;
            NSLog(@"%s", cString);
            char* _unityString = (char*)malloc(strlen(cString) + 1);
            strcpy(_unityString, cString);
            
            NSString* fn = [NSString stringWithUTF8String:"bloctowalletprovider"];
            NSString* failedFn = [NSString stringWithUTF8String:"UniversalLinkCallbackHandler"];
            UnitySendMessage([fn UTF8String], [failedFn UTF8String], _unityString);
            
            return _unityString;
        }
        catch (NSException *exception)
        {
          // Print exception information
          NSLog( @"NSException caught" );
          NSLog( @"Name: %@", exception.name);
          NSLog( @"Reason: %@", exception.reason );
          return "";
        }
    }
}

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
    self.URL = [NSString new];
}
@end

extern "C"
{
    
    void UniversalLink_Reset()
    { return [[UniversalLink instance] reset]; }

    const char* UniversalLink_GetURL()
    {
        const char* cString = [[UniversalLink instance] URL].UTF8String;
        char* _unityString = (char*)malloc(strlen(cString) + 1);
        strcpy(_unityString, cString);
        return _unityString;
        
    }
}
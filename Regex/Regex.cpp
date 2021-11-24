#include <boost/regex.hpp>
#include <strsafe.h>
#include <objbase.h>

// Who would have thought that I would reference the quest port?
// this is my revenge Fern! (also pls dont bully me, ive never written c++ before)
extern "C" {
    _declspec(dllexport) void LookupID_internal(char* ppStrArray[], int count, int** ppArray, int* pSize, char* id, int method)
    {
        std::function < bool(std::string const&) > predicate;
        boost::regex regex;

        switch (method) {
            case 0: {
                regex = boost::regex(id, boost::regex_constants::ECMAScript | boost::regex_constants::optimize);
                predicate = [&regex](const std::string& n) {
                    return boost::regex_search(n, regex);
                };
                break;
            }

            case 1: {
                predicate = [&id](const std::string& n) { return n == id; };
                break;
            }

            case 2: {
                predicate = [&id](const std::string& n) {
                    return n.find(id) != std::string::npos;
                };
                break;
            }

            default: {
                return;
            }
        }

        std::vector<int> ret;
        ret.reserve(count);

        for (int i = 0; i < count; i++)
        {
            std::string infoId(ppStrArray[i]);
            if (predicate(infoId))
                ret.emplace_back(i);
        }

        ret.shrink_to_fit();

        int newSize = ret.size();
        *pSize = newSize;

        int* newArray = (int*)CoTaskMemAlloc(sizeof(int) * newSize);
        for (size_t i = 0; i != newSize; i++) {
            newArray[i] = ret[i];
        }

        CoTaskMemFree(*ppArray);
        *ppArray = newArray;


        /*for (size_t i = 0; i != newSize; i++) {
            *ppArray[i] = ret[i];
        }*/

        return;
    }
}

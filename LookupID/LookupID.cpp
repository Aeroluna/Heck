#include <boost/regex.hpp>
#include <objbase.h>

#include <iostream>

enum class LookupMethod
{
    Regex,
    Exact,
    Contains,
    StartsWith,
    EndsWith
};

static std::vector<int> LookupID_detail(const char* id, const char* ppStrArray[], int count, LookupMethod method)
{
    auto match = [ppStrArray, count](auto&& predicate)
    {
        std::vector<int> ret;
        ret.reserve(count);
        for (int i = 0; i < count; ++i)
        {
            if (predicate(ppStrArray[i]))
            {
                ret.emplace_back(i);
            }
        }
        return ret;
    };
    switch (method)
    {
    case LookupMethod::Regex:
        return match(
            [regex = boost::regex{ id, boost::regex_constants::ECMAScript | boost::regex_constants::optimize }]
        (const std::string_view n){
            return boost::regex_search(n.data(), regex);
        });
    case LookupMethod::Exact:
        return match(
            [id]
        (const std::string_view n) {
                return n == id;
            });
    case LookupMethod::Contains:
        return match(
            [id]
        (const std::string_view n) {
                return n.find(id) != std::string::npos;
            });
    case LookupMethod::StartsWith:
        return match(
            [id]
        (const std::string_view n) {
                return n.starts_with(id);
            });
    case LookupMethod::EndsWith:
        return match(
            [id]
        (const std::string_view n) {
                return n.ends_with(id);
            });
    }
    return {};
}


// Who would have thought that I would reference the quest port?
// this is my revenge Fern! (also pls dont bully me, ive never written c++ before)
extern "C" {
    _declspec(dllexport) void LookupID_internal(const char* ppStrArray[], int count, int** ppArray, int* pSize, const char* id, LookupMethod method)
    {
        try {
            std::vector<int> ret = LookupID_detail(id, ppStrArray, count, method);
            size_t newSize = ret.size();
            *pSize = newSize;

            int* newArray = (int*)CoTaskMemAlloc(sizeof(int) * newSize);
            for (size_t i = 0; i != newSize; i++) {
                newArray[i] = ret[i];
            }

            *ppArray = newArray;
        }
        catch (std::exception const& e) {
            const char* exception = e.what();
            std::cout << "Regex exception! \"" << exception << "\"" << std::endl;
            return;
        }
    }
}

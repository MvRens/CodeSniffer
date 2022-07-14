// https://stackoverflow.com/a/69623589/2751830
// ...but I didn't want to pollute the String prototype
export function localeContains(value: string, substring: string): boolean
{
    if (substring === '')
        return true;

    if (!substring || !value.length) 
        return false;

    substring = '' + substring;
    if (substring.length > value.length) 
        return false;

    let ascii = (s: string) => s.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
    return ascii(value).includes(ascii(substring));
  }
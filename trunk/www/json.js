/*
    json.js

    Public Domain
    
    The global object JSON contains three methods.

    JSON.stringify(value) takes a JavaScript value and produces a JSON 
    text. The value must not be cyclical.

    JSON.parse(text) takes a JSON text and produces a JavaScript value. 
    It will throw SyntaxError if there is an error in parsing the JSON
    text. This method is fully JavaScript-based and therefore can be
    several magnitudes slower than JSON.eval. However, it is also more
    permissive than JSON.eval. For example, JSON.parse permits use of
    JavaScript-style comments in JSON text.

    JSON.eval(text) takes a JSON text and produces a JavaScript value.
    It will throw SyntaxError if there is an error in parsing the JSON
    text. JSON.eval is much faster (as it internally relies on the 
    JavaScript eval method after making safety checks) and stricter in 
    complaince to JSON text.
    
    NOTE: This implementation is hand-synchronized with the reference 
    implementation available over at http://www.json.org/json.js.
*/
var JSON = function () {
    var m = {
            '\b': '\\b',
            '\t': '\\t',
            '\n': '\\n',
            '\f': '\\f',
            '\r': '\\r',
            '"' : '\\"',
            '\\': '\\\\'
        },
        s = {
            'boolean': function (x) {
                return String(x);
            },
            number: function (x) {
                return isFinite(x) ? String(x) : 'null';
            },
            string: function (x) {
                // If the string contains no control characters, no quote 
                // characters, and no backslash characters, then we can 
                // simply slap some quotes around it. Otherwise we must 
                // also replace the offending characters with safe
                // sequences.
                if (/["\\\x00-\x1f]/.test(x)) {
                    x = x.replace(/[\x00-\x1f\\"]/g, function (a) {
                        var c = m[a];
                        if (c) {
                            return c;
                        }
                        c = a.charCodeAt();
                        return '\\u00' +
                            Math.floor(c / 16).toString(16) +
                            (c % 16).toString(16);
                    });
                }
                return '"' + x + '"';
            },
            object: function (x) {
                if (x) {
                    var a = [], b, f, i, l, v;
                    if (x instanceof Array) {
                        a[0] = '[';
                        l = x.length;
                        for (i = 0; i < l; i += 1) {
                            v = x[i];
                            f = s[typeof v];
                            if (f) {
                                v = f(v);
                                if (typeof v == 'string') {
                                    if (b) {
                                        a[a.length] = ',';
                                    }
                                    a[a.length] = v;
                                    b = true;
                                }
                            }
                        }
                        a[a.length] = ']';
                    } else if (x instanceof Date) {
                        function p(n) { return n < 10 ? '0' + n : n; };
                        var tz = x.getTimezoneOffset();
                        if (tz != 0) {
                            var tzh = Math.floor(Math.abs(tz) / 60);
                            var tzm = Math.abs(tz) % 60;
                            tz = (tz < 0 ? '+' : '-') + p(tzh) + ':' + p(tzm);
                        }
                        else {
                            tz = 'Z';
                        }
                        return '"' + 
                                x.getFullYear() + '-' +
                                p(x.getMonth() + 1) + '-' +
                                p(x.getDate()) + 'T' +
                                p(x.getHours()) + ':' +
                                p(x.getMinutes()) + ':' +
                                p(x.getSeconds()) + tz + '"';
                    } else if (x instanceof Object) {
                        a[0] = '{';
                        // Iterate through all of the keys in the 
                        // object, ignoring the proto chain and keys 
                        // that are not strings.
                        for (i in x) {
                            if (typeof i === 'string' && 
                                Object.prototype.hasOwnProperty.apply(x, [i])) {
                                v = x[i];
                                f = s[typeof v];
                                if (f) {
                                    v = f(v);
                                    if (typeof v == 'string') {
                                        if (b) {
                                            a[a.length] = ',';
                                        }
                                        a.push(s.string(i), ':', v);
                                        b = true;
                                    }
                                }
                            }
                        }
                        a[a.length] = '}';
                    } else {
                        return;
                    }
                    return a.join('');
                }
                return 'null';
            }
        };
    return {
        copyright: '(c)2005 JSON.org',
        license: 'http://www.crockford.com/JSON/license.html',
/*
    Stringify a JavaScript value, producing a JSON text.
*/
        stringify: function (v) {
            var f = s[typeof v];
            if (f) {
                v = f(v);
                if (typeof v == 'string') {
                    return v;
                }
            }
            return null;
        },
/*
    Parse a JSON text, producing a JavaScript value.
    If the text is not JSON parseable, then a SyntaxError is thrown.
*/
        eval: function (text, filter) {

            function walk(k, v) {
                var i;
                if (v && typeof v === 'object') {
                    for (i in v) {
                        if (Object.prototype.hasOwnProperty.apply(v, [i])) {
                            v[i] = walk(i, v[i]);
                        }
                    }
                }
                return filter(k, v);
            }

            // Parsing happens in three stages. In the first stage, we run the
            // text against a regular expression which looks for non-JSON
            // characters. We are especially concerned with '()' and 'new'
            // because they can cause invocation, and '=' because it can cause
            // mutation. But just to be safe, we will reject all unexpected
            // characters.

            // We split the first stage into 3 regexp operations in order to
            // work around crippling deficiencies in Safari's regexp engine.
            // First we replace all backslash pairs with '@' (a non-JSON
            // character). Second we delete all of the string literals. Third,
            // we look to see if any non-JSON characters remain. If not, then
            // the text is safe for eval.

            if (!/^[,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t]*$/.test(text.
                replace(/\\./g, '@').
                replace(/"[^"\\\n\r]*"/g, ''))) {
                throw new SyntaxError("eval");
            }

            // In the second stage we use the eval function to compile the 
            // text into a JavaScript structure. The '{' operator is subject 
            // to a syntactic ambiguity in JavaScript: it can begin a block 
            // or an object literal. We wrap the text in parens to eliminate 
            // the ambiguity.

            var result = eval('(' + text + ')');

            // In the optional third stage, we recursively walk the new
            // structure, passing each name/value pair to a filter function 
            // for possible transformation.

            if (typeof filter === 'function')
                result = walk('', result);

            return result;
        },

        parse: function (text) {
            var at = 0;
            var ch = ' ';

            function error(m) {
                var e = new SyntaxError(m);
                e.at = at - 1;
                e.text = text;
                throw e;
            }

            function next() {
                ch = text.charAt(at);
                at += 1;
                return ch;
            }

            function white() {
                while (ch) {
                    if (ch <= ' ') {
                        next();
                    } else if (ch == '/') {
                        switch (next()) {
                            case '/':
                                while (next() && ch != '\n' && ch != '\r') {}
                                break;
                            case '*':
                                next();
                                for (;;) {
                                    if (ch) {
                                        if (ch == '*') {
                                            if (next() == '/') {
                                                next();
                                                break;
                                            }
                                        } else {
                                            next();
                                        }
                                    } else {
                                        error("Unterminated comment");
                                    }
                                }
                                break;
                            default:
                                error("Syntax error");
                        }
                    } else {
                        break;
                    }
                }
            }

            function string() {
                var i, s = '', t, u;

                if (ch == '"') {
    outer:          while (next()) {
                        if (ch == '"') {
                            next();
                            return s;
                        } else if (ch == '\\') {
                            switch (next()) {
                            case 'b':
                                s += '\b';
                                break;
                            case 'f':
                                s += '\f';
                                break;
                            case 'n':
                                s += '\n';
                                break;
                            case 'r':
                                s += '\r';
                                break;
                            case 't':
                                s += '\t';
                                break;
                            case 'u':
                                u = 0;
                                for (i = 0; i < 4; i += 1) {
                                    t = parseInt(next(), 16);
                                    if (!isFinite(t)) {
                                        break outer;
                                    }
                                    u = u * 16 + t;
                                }
                                s += String.fromCharCode(u);
                                break;
                            default:
                                s += ch;
                            }
                        } else {
                            s += ch;
                        }
                    }
                }
                error("Bad string");
            }

            function array() {
                var a = [];

                if (ch == '[') {
                    next();
                    white();
                    if (ch == ']') {
                        next();
                        return a;
                    }
                    while (ch) {
                        a.push(value());
                        white();
                        if (ch == ']') {
                            next();
                            return a;
                        } else if (ch != ',') {
                            break;
                        }
                        next();
                        white();
                    }
                }
                error("Bad array");
            }

            function object() {
                var k, o = {};

                if (ch == '{') {
                    next();
                    white();
                    if (ch == '}') {
                        next();
                        return o;
                    }
                    while (ch) {
                        k = string();
                        white();
                        if (ch != ':') {
                            break;
                        }
                        next();
                        o[k] = value();
                        white();
                        if (ch == '}') {
                            next();
                            return o;
                        } else if (ch != ',') {
                            break;
                        }
                        next();
                        white();
                    }
                }
                error("Bad object");
            }

            function number() {
                var n = '', v;
                if (ch == '-') {
                    n = '-';
                    next();
                }
                while (ch >= '0' && ch <= '9') {
                    n += ch;
                    next();
                }
                if (ch == '.') {
                    n += '.';
                    while (next() && ch >= '0' && ch <= '9') {
                        n += ch;
                    }
                }
                if (ch == 'e' || ch == 'E') {
                    n += 'e';
                    next();
                    if (ch == '-' || ch == '+') {
                        n += ch;
                        next();
                    }
                    while (ch >= '0' && ch <= '9') {
                        n += ch;
                        next();
                    }
                }
                v = +n;
                if (!isFinite(v)) {
                    ////error("Bad number");
                } else {
                    return v;
                }
            }

            function word() {
                switch (ch) {
                    case 't':
                        if (next() == 'r' && next() == 'u' && next() == 'e') {
                            next();
                            return true;
                        }
                        break;
                    case 'f':
                        if (next() == 'a' && next() == 'l' && next() == 's' &&
                                next() == 'e') {
                            next();
                            return false;
                        }
                        break;
                    case 'n':
                        if (next() == 'u' && next() == 'l' && next() == 'l') {
                            next();
                            return null;
                        }
                        break;
                }
                error("Syntax error");
            }

            function value() {
                white();
                switch (ch) {
                    case '{':
                        return object();
                    case '[':
                        return array();
                    case '"':
                        return string();
                    case '-':
                        return number();
                    default:
                        return ch >= '0' && ch <= '9' ? number() : word();
                }
            }

            return value();
        }
    };
}();
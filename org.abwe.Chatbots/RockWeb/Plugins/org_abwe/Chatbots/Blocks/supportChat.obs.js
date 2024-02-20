System.register(['vue', '@Obsidian/PageState', '@Obsidian/Utility/block', '@Obsidian/Utility/guid', 'https://cdn.jsdelivr.net/npm/showdown@2.1.0/dist/showdown.min.js'], (function (exports) {
  'use strict';
  var createTextVNode, defineComponent, ref, onMounted, onUnmounted, openBlock, createElementBlock, normalizeClass, createVNode, Transition, withCtx, createElementVNode, createCommentVNode, Fragment, renderList, toDisplayString, withDirectives, withKeys, vModelText, pushScopeId, popScopeId, nextTick, useStore, useInvokeBlockAction, useConfigurationValues, useBlockGuid, newGuid;
  return {
    setters: [function (module) {
      createTextVNode = module.createTextVNode;
      defineComponent = module.defineComponent;
      ref = module.ref;
      onMounted = module.onMounted;
      onUnmounted = module.onUnmounted;
      openBlock = module.openBlock;
      createElementBlock = module.createElementBlock;
      normalizeClass = module.normalizeClass;
      createVNode = module.createVNode;
      Transition = module.Transition;
      withCtx = module.withCtx;
      createElementVNode = module.createElementVNode;
      createCommentVNode = module.createCommentVNode;
      Fragment = module.Fragment;
      renderList = module.renderList;
      toDisplayString = module.toDisplayString;
      withDirectives = module.withDirectives;
      withKeys = module.withKeys;
      vModelText = module.vModelText;
      pushScopeId = module.pushScopeId;
      popScopeId = module.popScopeId;
      nextTick = module.nextTick;
    }, function (module) {
      useStore = module.useStore;
    }, function (module) {
      useInvokeBlockAction = module.useInvokeBlockAction;
      useConfigurationValues = module.useConfigurationValues;
      useBlockGuid = module.useBlockGuid;
    }, function (module) {
      newGuid = module.newGuid;
    }, function () {}],
    execute: (function () {

      function asyncGeneratorStep(gen, resolve, reject, _next, _throw, key, arg) {
        try {
          var info = gen[key](arg);
          var value = info.value;
        } catch (error) {
          reject(error);
          return;
        }
        if (info.done) {
          resolve(value);
        } else {
          Promise.resolve(value).then(_next, _throw);
        }
      }
      function _asyncToGenerator(fn) {
        return function () {
          var self = this,
            args = arguments;
          return new Promise(function (resolve, reject) {
            var gen = fn.apply(self, args);
            function _next(value) {
              asyncGeneratorStep(gen, resolve, reject, _next, _throw, "next", value);
            }
            function _throw(err) {
              asyncGeneratorStep(gen, resolve, reject, _next, _throw, "throw", err);
            }
            _next(undefined);
          });
        };
      }

      var _withScopeId = n => (pushScopeId("data-v-da67af10"), n = n(), popScopeId(), n);
      var _hoisted_1 = {
        key: 0,
        class: "chat-panel card"
      };
      var _hoisted_2 = {
        class: "header bg-primary"
      };
      var _hoisted_3 = _withScopeId(() => createElementVNode("h3", null, [createTextVNode("Chat with "), createElementVNode("i", {
        class: "fal fa-robot"
      })], -1));
      var _hoisted_4 = _withScopeId(() => createElementVNode("i", {
        class: "fal fa-expand"
      }, null, -1));
      var _hoisted_5 = [_hoisted_4];
      var _hoisted_6 = _withScopeId(() => createElementVNode("div", {
        class: "ai-warning",
        style: {
          "line-height": "1"
        }
      }, [createElementVNode("i", {
        class: "fa fa-exclamation-triangle pull-left mr-3 text-muted"
      }), createTextVNode(" Computer generated responses may not be accurate. Please refer to the policies for more information. ")], -1));
      var _hoisted_7 = _withScopeId(() => createElementVNode("div", {
        class: "response-header"
      }, [createElementVNode("i", {
        class: "fa fa-robot"
      }), createTextVNode(" ABWE Bot ")], -1));
      var _hoisted_8 = _withScopeId(() => createElementVNode("div", {
        class: "response bot"
      }, " What can I help you with? You can ask questions like, \"How do I submit an expense report?\", \"What is ABWEs stance on language learning?\", or \"What are my current benefits with ABWE?\" ", -1));
      var _hoisted_9 = {
        key: 0,
        class: "response-header bot"
      };
      var _hoisted_10 = _withScopeId(() => createElementVNode("i", {
        class: "fa fa-robot"
      }, null, -1));
      var _hoisted_11 = createTextVNode(" ABWE Bot ");
      var _hoisted_12 = [_hoisted_10, _hoisted_11];
      var _hoisted_13 = {
        key: 1,
        class: "response-header human"
      };
      var _hoisted_14 = _withScopeId(() => createElementVNode("i", {
        class: "fa fa-user"
      }, null, -1));
      var _hoisted_15 = createTextVNode(" You ");
      var _hoisted_16 = [_hoisted_14, _hoisted_15];
      var _hoisted_17 = createTextVNode(" Here are some resources that might help:");
      var _hoisted_18 = _withScopeId(() => createElementVNode("br", null, null, -1));
      var _hoisted_19 = ["href"];
      var _hoisted_20 = ["innerHTML"];
      var _hoisted_21 = _withScopeId(() => createElementVNode("div", {
        class: "typing-indicator"
      }, [createElementVNode("span"), createElementVNode("span"), createElementVNode("span")], -1));
      var _hoisted_22 = [_hoisted_21];
      var _hoisted_23 = {
        key: 0,
        class: "response bot"
      };
      var _hoisted_24 = {
        class: "input"
      };
      var _hoisted_25 = {
        class: "input-group"
      };
      var _hoisted_26 = ["onKeyup"];
      var _hoisted_27 = {
        class: "input-group-btn"
      };
      var _hoisted_28 = _withScopeId(() => createElementVNode("i", {
        class: "fal fa-paper-plane"
      }, null, -1));
      var _hoisted_29 = [_hoisted_28];
      var _hoisted_30 = _withScopeId(() => createElementVNode("i", {
        class: "fa fa-square"
      }, null, -1));
      var _hoisted_31 = [_hoisted_30];
      var _hoisted_32 = {
        key: 0,
        class: "fas fa-comment"
      };
      var _hoisted_33 = {
        key: 1,
        class: "fas fa-times"
      };
      var script = exports('default', defineComponent({
        name: 'supportChat',
        setup(__props) {
          useInvokeBlockAction();
          var config = useConfigurationValues();
          var guid = useBlockGuid();
          var store = useStore();
          var conversationHistory = ref([]);
          var currentMessage = ref('');
          var chatPanelVisible = ref(false);
          var historyRef = ref(null);
          var chatRoot = ref(null);
          var responding = ref(false);
          var error = ref("");
          var sessionId = newGuid();
          var isFullscreen = ref(false);
          var floating = ref(config.style == "Popup");
          var abortController = new AbortController();
          function markdownToHtml(markdown) {
            var converter = new showdown.Converter({
              openLinksInNewWindow: true
            });
            return converter.makeHtml(markdown);
          }
          function updateFullscreenState() {
            isFullscreen.value = document.fullscreenElement === chatRoot.value;
          }
          onMounted(() => {
            document.addEventListener('fullscreenchange', updateFullscreenState);
          });
          onUnmounted(() => {
            document.removeEventListener('fullscreenchange', updateFullscreenState);
          });
          function requestFullScreen() {
            var _chatRoot$value;
            (_chatRoot$value = chatRoot.value) === null || _chatRoot$value === void 0 || _chatRoot$value.requestFullscreen();
          }
          function abortFetch() {
            if (responding.value) {
              abortController.abort();
              responding.value = false;
            }
          }
          function sendMessage() {
            return _sendMessage.apply(this, arguments);
          }
          function _sendMessage() {
            _sendMessage = _asyncToGenerator(function* () {
              if (responding.value == true) return;
              var message = currentMessage.value;
              currentMessage.value = '';
              error.value = '';
              conversationHistory.value.push({
                content: message,
                type: 'human'
              });
              conversationHistory.value.push({
                content: '',
                type: 'bot'
              });
              nextTick(() => {
                if (historyRef.value) historyRef.value.scrollTop = historyRef.value.scrollHeight;
              });
              try {
                abortController = new AbortController();
                responding.value = true;
                var response = yield fetch("/api/v2/BlockActions/".concat(store.state.pageGuid, "/").concat(guid, "/GetMessage"), {
                  method: 'POST',
                  headers: {
                    'Content-Type': 'application/json'
                  },
                  body: JSON.stringify({
                    message: message,
                    sessionId: sessionId
                  }),
                  signal: abortController.signal
                });
                if (!response.body) return;
                var reader = response.body.getReader();
                var decoder = new TextDecoder("utf-8");
                var result = '';
                var finalString = '';
                while (true) {
                  var _yield$reader$read = yield reader.read(),
                    done = _yield$reader$read.done,
                    value = _yield$reader$read.value;
                  if (done) {
                    break;
                  }
                  result += decoder.decode(value);
                  var endOfEvent = result.indexOf('\r\n');
                  while (endOfEvent !== -1) {
                    var line = result.slice(0, endOfEvent);
                    result = result.slice(endOfEvent + 2);
                    if (line.startsWith('data: ')) {
                      var data = line.slice(6);
                      if (data.trim() == '' || data.trim() == 'null') continue;
                      var autoScroll = false;
                      if (historyRef.value) {
                        var _historyRef$value, _historyRef$value2, _historyRef$value3;
                        autoScroll = ((_historyRef$value = historyRef.value) === null || _historyRef$value === void 0 ? void 0 : _historyRef$value.scrollHeight) - ((_historyRef$value2 = historyRef.value) === null || _historyRef$value2 === void 0 ? void 0 : _historyRef$value2.clientHeight) == Math.ceil((_historyRef$value3 = historyRef.value) === null || _historyRef$value3 === void 0 ? void 0 : _historyRef$value3.scrollTop);
                      }
                      if (data.startsWith("DESCRIPTOR:")) {
                        conversationHistory.value[conversationHistory.value.length - 1].sources = JSON.parse(data.slice(11)).reduce((acc, curr) => {
                          var duplicate = acc.find(s => s.id === curr.id);
                          if (!duplicate) {
                            acc.push(curr);
                          }
                          return acc;
                        }, []);
                      } else {
                        finalString += JSON.parse(data);
                        conversationHistory.value[conversationHistory.value.length - 1].content = finalString;
                      }
                      if (autoScroll) {
                        nextTick(() => {
                          if (historyRef.value) historyRef.value.scrollTop = historyRef.value.scrollHeight;
                        });
                      }
                    } else {
                      console.error('Error parsing event:', line);
                    }
                    endOfEvent = result.indexOf('\r\n');
                  }
                }
                responding.value = false;
              } catch (e) {
                console.error('Error:', e);
              }
            });
            return _sendMessage.apply(this, arguments);
          }
          return (_ctx, _cache) => {
            return openBlock(), createElementBlock("div", {
              class: normalizeClass(["chat-root", {
                floating: floating.value,
                fullscreen: isFullscreen.value
              }]),
              ref_key: "chatRoot",
              ref: chatRoot
            }, [createVNode(Transition, {
              name: "slide-fade"
            }, {
              default: withCtx(() => [!floating.value || chatPanelVisible.value ? (openBlock(), createElementBlock("div", _hoisted_1, [createElementVNode("div", _hoisted_2, [_hoisted_3, !isFullscreen.value ? (openBlock(), createElementBlock("div", {
                key: 0,
                class: "btn btn-link",
                onClick: requestFullScreen
              }, _hoisted_5)) : createCommentVNode("v-if", true)]), _hoisted_6, createElementVNode("div", {
                class: "history",
                ref_key: "historyRef",
                ref: historyRef
              }, [_hoisted_7, _hoisted_8, (openBlock(true), createElementBlock(Fragment, null, renderList(conversationHistory.value, (message, index) => {
                return openBlock(), createElementBlock(Fragment, {
                  key: index
                }, [message.type == 'bot' ? (openBlock(), createElementBlock("div", _hoisted_9, _hoisted_12)) : createCommentVNode("v-if", true), message.type == 'human' ? (openBlock(), createElementBlock("div", _hoisted_13, _hoisted_16)) : createCommentVNode("v-if", true), message.sources ? (openBlock(), createElementBlock("div", {
                  key: 2,
                  class: normalizeClass(['response', message.type])
                }, [_hoisted_17, _hoisted_18, createElementVNode("ul", null, [(openBlock(true), createElementBlock(Fragment, null, renderList(message.sources, source => {
                  return openBlock(), createElementBlock("li", null, [createElementVNode("a", {
                    href: source.url
                  }, toDisplayString(source.name), 9, _hoisted_19)]);
                }), 256))])], 2)) : createCommentVNode("v-if", true), createCommentVNode(" <a v-if=\"message.sources\" v-for=\"source in message.sources\" :href=\"source.url\" :class=\"['response', 'article', message.type]\">\r\n                            <i class=\"fal fa-file-alt fa-2x mr-3\"></i>\r\n                            {{ source.name }}\r\n                        </a> "), message.content ? (openBlock(), createElementBlock("div", {
                  key: 3,
                  class: normalizeClass(['response', message.type]),
                  innerHTML: markdownToHtml(message.content)
                }, null, 10, _hoisted_20)) : createCommentVNode("v-if", true), !message.content ? (openBlock(), createElementBlock("div", {
                  key: 4,
                  class: normalizeClass(['response', message.type])
                }, _hoisted_22, 2)) : createCommentVNode("v-if", true)], 64);
              }), 128)), error.value ? (openBlock(), createElementBlock("div", _hoisted_23, toDisplayString(error.value), 1)) : createCommentVNode("v-if", true), createCommentVNode(" <button class=\"btn btn-danger btn-link stop-button\" v-if=\"responding\" @click=\"abortFetch\"><i class=\"fa fa-square\"></i> Stop</button> ")], 512), createElementVNode("div", _hoisted_24, [createElementVNode("div", _hoisted_25, [withDirectives(createElementVNode("input", {
                class: "form-control",
                placeholder: "Ask a question...",
                "onUpdate:modelValue": _cache[0] || (_cache[0] = $event => currentMessage.value = $event),
                onKeyup: withKeys(sendMessage, ["enter"])
              }, null, 40, _hoisted_26), [[vModelText, currentMessage.value]]), createElementVNode("span", _hoisted_27, [!responding.value ? (openBlock(), createElementBlock("button", {
                key: 0,
                class: "btn btn-default text-muted",
                type: "button",
                onClick: sendMessage,
                style: {
                  "border-left": "0"
                }
              }, _hoisted_29)) : createCommentVNode("v-if", true), responding.value ? (openBlock(), createElementBlock("button", {
                key: 1,
                class: "btn btn-default text-muted",
                type: "button",
                onClick: abortFetch,
                style: {
                  "border-left": "0"
                }
              }, _hoisted_31)) : createCommentVNode("v-if", true)])]), createCommentVNode(" /input-group ")])])) : createCommentVNode("v-if", true)]),
              _: 1
            }), floating.value ? (openBlock(), createElementBlock("div", {
              key: 0,
              class: "btn btn-primary open-chat",
              onClick: _cache[1] || (_cache[1] = $event => chatPanelVisible.value = !chatPanelVisible.value)
            }, [!chatPanelVisible.value ? (openBlock(), createElementBlock("i", _hoisted_32)) : (openBlock(), createElementBlock("i", _hoisted_33))])) : createCommentVNode("v-if", true)], 2);
          };
        }
      }));

      function styleInject(css, ref) {
        if (ref === void 0) ref = {};
        var insertAt = ref.insertAt;
        if (!css || typeof document === 'undefined') {
          return;
        }
        var head = document.head || document.getElementsByTagName('head')[0];
        var style = document.createElement('style');
        style.type = 'text/css';
        if (insertAt === 'top') {
          if (head.firstChild) {
            head.insertBefore(style, head.firstChild);
          } else {
            head.appendChild(style);
          }
        } else {
          head.appendChild(style);
        }
        if (style.styleSheet) {
          style.styleSheet.cssText = css;
        } else {
          style.appendChild(document.createTextNode(css));
        }
      }

      var css_248z = ".chat-root[data-v-da67af10]{align-items:flex-end;display:flex;flex-direction:column}.chat-root.floating[data-v-da67af10]{bottom:2rem;position:fixed;right:1rem;z-index:2000}.chat-root.floating .open-chat[data-v-da67af10]{align-items:center;border-radius:50%;box-shadow:0 0 10px 0 #b3b3b3;display:flex;height:50px;justify-content:center;width:50px}.chat-panel[data-v-da67af10]{background:#fff;box-shadow:0 0 1px 0 rgba(0,0,0,.03),0 1px 3px 0 rgba(0,0,0,.08);display:flex;flex-direction:column;height:500px;margin-bottom:1rem;overflow:hidden;width:100%}.chat-root.fullscreen .chat-panel[data-v-da67af10]{height:100vh}.chat-root.floating .chat-panel[data-v-da67af10]{border-radius:.5rem;box-shadow:0 0 10px 0 rgba(0,0,0,.5);height:700px;max-height:100vh;max-width:100vw;width:700px}.chat-panel .history[data-v-da67af10]{display:flex;flex-direction:column;flex-grow:1;gap:.5rem;overflow-y:auto;padding:1rem;position:relative}.chat-panel .input[data-v-da67af10]{background:#efefef;padding:1rem}.response-header[data-v-da67af10]{color:#737475;font-size:.75em}.response-header.bot[data-v-da67af10]{align-self:flex-start}.response-header.human[data-v-da67af10]{align-self:flex-end}.response[data-v-da67af10]{border-radius:.5rem;max-width:75%;padding:1rem}[data-v-da67af10] .response p:last-child{margin-bottom:0}.response.human[data-v-da67af10]{align-self:flex-end;background:#f0f0f0;text-align:left}.response.bot[data-v-da67af10]{align-self:flex-start;background:#d0f0ff;text-align:left}a.response.article[data-v-da67af10]{color:#000}a.response.article i[data-v-da67af10]{vertical-align:middle}.slide-fade-enter-active[data-v-da67af10],.slide-fade-leave-active[data-v-da67af10]{transition:transform .2s ease,opacity .2s ease}.slide-fade-enter-from[data-v-da67af10],.slide-fade-leave-to[data-v-da67af10]{opacity:0;transform:translateY(100px)}.ai-warning[data-v-da67af10]{align-items:center;align-self:center;display:flex;font-size:.75rem;margin-top:1rem;text-align:center}.stop-button[data-v-da67af10]{align-self:center;bottom:1rem;margin-top:auto;width:100px}.header[data-v-da67af10]{align-items:center;display:flex;justify-content:space-between;padding:.5rem 1rem}.header .btn[data-v-da67af10]{color:#fff}.header h3[data-v-da67af10]{font-size:1rem;margin:0;padding:0}.typing-indicator[data-v-da67af10]{align-items:center;display:flex;justify-content:center}.typing-indicator span[data-v-da67af10]{animation:typing-da67af10 1.4s infinite both;background-color:#9e9e9e;border-radius:50%;display:inline-block;height:8px;margin:0 2px;width:8px}.typing-indicator span[data-v-da67af10]:first-child{animation-delay:-.32s}.typing-indicator span[data-v-da67af10]:nth-child(2){animation-delay:-.16s}@keyframes typing-da67af10{0%,80%,to{transform:scale(0)}40%{transform:scale(1)}}";
      styleInject(css_248z);

      script.__scopeId = "data-v-da67af10";
      script.__file = "src/supportChat.obs";

    })
  };
}));
//# sourceMappingURL=supportChat.obs.js.map
